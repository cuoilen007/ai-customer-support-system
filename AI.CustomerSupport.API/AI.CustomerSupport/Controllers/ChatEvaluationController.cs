using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.AI;
using AI.CustomerSupport.API.DTOs.Chat;
using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ChatEvaluationController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ChatEvaluationController> _logger;
        private readonly IAiService _aiService;

        public ChatEvaluationController(
            AppDbContext context,
            ILogger<ChatEvaluationController> logger,
            IAiService aiService)
        {
            _context = context;
            _logger = logger;
            _aiService = aiService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(bool needsReviewOnly = false)
        {
            try
            {
                var userId = ClaimsHelper.GetUserId(User);

                await BackfillMissingEvaluationsAsync(userId);

                var query = _context.ChatEvaluations
                    .Include(x => x.Conversation)
                    .Include(x => x.UserMessage)
                    .Include(x => x.AssistantMessage)
                    .Where(x =>
                        !x.IsDeleted
                        && !x.KnowledgeGapResolved
                        && x.Conversation != null
                        && x.Conversation.UserId == userId);

                if (needsReviewOnly)
                {
                    query = query.Where(x => x.NeedsHumanReview);
                }

                var evaluations = await query
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new
                    {
                        x.Id,
                        x.ConversationId,
                        UserQuestion = x.UserMessage == null ? string.Empty : x.UserMessage.Content,
                        AssistantAnswer = x.AssistantMessage == null ? string.Empty : x.AssistantMessage.Content,
                        x.Category,
                        x.Sentiment,
                        x.Intent,
                        x.ConfidenceScore,
                        x.NeedsHumanReview,
                        x.PrimarySourceId,
                        x.PrimarySourceType,
                        x.RetrievedSourcesJson,
                        x.ImprovementNote,
                        x.ApprovedForTraining,
                        x.KnowledgeGap,
                        x.HumanCorrectedAnswer,
                        x.IsDeleted,
                        x.CreatedAt
                    })
                    .ToListAsync();

                return Ok(evaluations);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not load chat evaluations. Returning an empty list.");

                return Ok(Array.Empty<object>());
            }
        }

        [HttpGet("training-data")]
        public async Task<IActionResult> GetTrainingData()
        {
            try
            {
                var userId = ClaimsHelper.GetUserId(User);
                var status = await _aiService.GetTrainingStatusAsync();

                await BackfillMissingEvaluationsAsync(userId);
                await SyncTrainingExamplesAsync(userId);
                await SyncQueuedTrainingExamplesAsync(userId, status);
                await SyncTrainingRunsAsync(userId, status);

                var rows = await BuildReviewTrainingExamplesQuery(userId)
                    .Select(x => new ModelTrainingExampleExportItem
                    {
                        Id = x.Id,
                        Input = x.Input,
                        Output = x.ExpectedOutput,
                        OriginalAnswer = x.OriginalAnswer,
                        Intent = x.Intent,
                        Status = x.Status
                    })
                    .ToListAsync();

                return Ok(rows);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not load AI training data. Returning an empty list.");

                return Ok(Array.Empty<object>());
            }
        }

        [HttpGet("training-data/status")]
        public async Task<IActionResult> GetTrainingStatus()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var status = await _aiService.GetTrainingStatusAsync();

            if (status == null)
            {
                return Ok(new AiTrainingStatusResponse
                {
                    Status = "unavailable",
                    Message = "Training service is not available."
                });
            }

            await SyncQueuedTrainingExamplesAsync(userId, status);
            await SyncTrainingRunsAsync(userId, status);

            return Ok(status);
        }

        [HttpPost("training-data/run")]
        public async Task<IActionResult> RunTraining()
        {
            var userId = ClaimsHelper.GetUserId(User);
            var currentStatus = await _aiService.GetTrainingStatusAsync();

            await BackfillMissingEvaluationsAsync(userId);
            await SyncTrainingExamplesAsync(userId);
            await SyncQueuedTrainingExamplesAsync(userId, currentStatus);
            await SyncTrainingRunsAsync(userId, currentStatus);

            var trainingRows = await BuildTrainingDatasetQuery(userId)
                .ToListAsync();

            if (trainingRows.Count == 0)
            {
                return BadRequest(new
                {
                    message = "No finalized training examples are available yet."
                });
            }

            var queuedRows = trainingRows
                .Where(x => x.Status == "Ready")
                .ToList();

            foreach (var row in queuedRows)
            {
                row.Status = "Training";
                row.UpdatedAt = DateTime.UtcNow;
            }

            if (queuedRows.Count > 0)
            {
                await _context.SaveChangesAsync();
            }

            var examples = trainingRows
                .Select(x => new AiTrainingExampleRequest
                {
                    Input = x.Input,
                    Output = x.ExpectedOutput,
                    Category = x.Category,
                    Intent = x.Intent
                })
                .ToList();

            var status = await _aiService.RunTrainingAsync(
                new AiTrainingRunRequest
                {
                    Examples = examples
                });

            if (status == null)
            {
                foreach (var row in queuedRows)
                {
                    row.Status = "Ready";
                    row.UpdatedAt = DateTime.UtcNow;
                }

                if (queuedRows.Count > 0)
                {
                    await _context.SaveChangesAsync();
                }

                return Problem(
                    title: "Could not start model training.",
                    statusCode: StatusCodes.Status502BadGateway);
            }

            await RecordTrainingRunAsync(
                userId,
                status,
                queuedRows.Count(x => x.Source == "ReviewWorkspace"),
                trainingRows.Count,
                examples
                    .Select(x => x.Output.Trim())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .Count());

            return Ok(status);
        }

        [HttpPut("{id}/feedback")]
        public async Task<IActionResult> UpdateFeedback(
            int id,
            UpdateChatEvaluationFeedbackRequest request)
        {
            var userId = ClaimsHelper.GetUserId(User);

            var evaluation = await _context.ChatEvaluations
                .Include(x => x.Conversation)
                .Include(x => x.UserMessage)
                .Include(x => x.AssistantMessage)
                .FirstOrDefaultAsync(x =>
                    x.Id == id
                    && x.Conversation != null
                    && x.Conversation.UserId == userId);

            if (evaluation == null)
            {
                return NotFound();
            }

            try
            {
                evaluation.ApprovedForTraining = request.ApprovedForTraining;
                evaluation.KnowledgeGap = request.KnowledgeGap;
                evaluation.KnowledgeGapResolved = false;
                evaluation.HumanCorrectedAnswer =
                    (request.HumanCorrectedAnswer ?? string.Empty).Trim();

                await _context.SaveChangesAsync();
                await SyncTrainingExampleAsync(evaluation);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not save chat evaluation feedback.");

                return Problem(
                    title: "Could not save chat evaluation feedback.",
                    statusCode: StatusCodes.Status500InternalServerError);
            }

            return Ok(new
            {
                evaluation.Id,
                evaluation.ApprovedForTraining,
                evaluation.KnowledgeGap,
                evaluation.HumanCorrectedAnswer
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = ClaimsHelper.GetUserId(User);

            var evaluation = await _context.ChatEvaluations
                .Include(x => x.Conversation)
                .FirstOrDefaultAsync(x =>
                    x.Id == id
                    && x.Conversation != null
                    && x.Conversation.UserId == userId);

            if (evaluation == null)
            {
                return NotFound();
            }

            evaluation.IsDeleted = true;
            await SyncTrainingExampleAsync(evaluation);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("knowledge-gaps/resolve")]
        public async Task<IActionResult> ResolveKnowledgeGaps(
            ResolveKnowledgeGapRequest request)
        {
            var userId = ClaimsHelper.GetUserId(User);

            if (request.EvaluationIds.Count == 0)
            {
                return Ok(new { resolved = 0 });
            }

            var evaluations = await _context.ChatEvaluations
                .Include(x => x.Conversation)
                .Where(x =>
                    request.EvaluationIds.Contains(x.Id)
                    && x.Conversation != null
                    && x.Conversation.UserId == userId)
                .ToListAsync();

            foreach (var evaluation in evaluations)
            {
                evaluation.KnowledgeGapResolved = true;
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                resolved = evaluations.Count
            });
        }

        private async Task SyncTrainingExamplesAsync(int userId)
        {
            var evaluations = await _context.ChatEvaluations
                .Include(x => x.Conversation)
                .Include(x => x.UserMessage)
                .Include(x => x.AssistantMessage)
                .Where(x =>
                    x.Conversation != null
                    && x.Conversation.UserId == userId)
                .ToListAsync();

            foreach (var evaluation in evaluations)
            {
                await SyncTrainingExampleAsync(evaluation);
            }
        }

        private async Task SyncTrainingExampleAsync(ChatEvaluation evaluation)
        {
            var existing = await _context.ModelTrainingExamples
                .FirstOrDefaultAsync(x => x.ChatEvaluationId == evaluation.Id);

            var input = evaluation.UserMessage?.Content.Trim() ?? string.Empty;
            var expectedOutput = evaluation.HumanCorrectedAnswer.Trim();
            var originalAnswer = evaluation.AssistantMessage?.Content.Trim() ?? string.Empty;
            var hasMaterialChanges =
                existing == null
                || !string.Equals(existing.Input, input, StringComparison.Ordinal)
                || !string.Equals(existing.ExpectedOutput, expectedOutput, StringComparison.Ordinal)
                || !string.Equals(existing.OriginalAnswer, originalAnswer, StringComparison.Ordinal)
                || !string.Equals(existing.Category, evaluation.Category, StringComparison.Ordinal)
                || !string.Equals(existing.Intent, evaluation.Intent, StringComparison.Ordinal)
                || !string.Equals(existing.PrimarySourceId, evaluation.PrimarySourceId, StringComparison.Ordinal)
                || !string.Equals(existing.PrimarySourceType, evaluation.PrimarySourceType, StringComparison.Ordinal);
            var shouldBeActive =
                !evaluation.IsDeleted
                && evaluation.ApprovedForTraining
                && !evaluation.KnowledgeGap
                && !string.IsNullOrWhiteSpace(input)
                && !string.IsNullOrWhiteSpace(expectedOutput);

            if (!shouldBeActive)
            {
                if (existing != null)
                {
                    existing.IsActive = false;
                    existing.Status = "Inactive";
                    existing.UpdatedAt = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                }

                return;
            }

            if (existing == null)
            {
                existing = new ModelTrainingExample
                {
                    ChatEvaluationId = evaluation.Id,
                    Source = "ReviewWorkspace",
                    SourceReference = $"chat-evaluation:{evaluation.Id}",
                    CreatedAt = DateTime.UtcNow
                };

                _context.ModelTrainingExamples.Add(existing);
            }

            existing.Input = input;
            existing.ExpectedOutput = expectedOutput;
            existing.OriginalAnswer = originalAnswer;
            existing.Category = evaluation.Category;
            existing.Intent = evaluation.Intent;
            existing.PrimarySourceId = evaluation.PrimarySourceId;
            existing.PrimarySourceType = evaluation.PrimarySourceType;
            existing.IsActive = true;
            existing.Source = "ReviewWorkspace";
            existing.SourceReference = $"chat-evaluation:{evaluation.Id}";

            if (existing.Status == "Training" || existing.Status == "Trained")
            {
                if (hasMaterialChanges)
                {
                    existing.Status = "Ready";
                }
            }
            else
            {
                existing.Status = "Ready";
            }

            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private IQueryable<ModelTrainingExample> BuildReviewTrainingExamplesQuery(int userId)
        {
            return _context.ModelTrainingExamples
                .Include(x => x.ChatEvaluation)
                .ThenInclude(x => x!.Conversation)
                .Where(x =>
                    x.IsActive
                    && x.Status == "Ready"
                    && x.Source == "ReviewWorkspace"
                    && x.ChatEvaluation != null
                    && x.ChatEvaluation.Conversation != null
                    && x.ChatEvaluation.Conversation.UserId == userId)
                .OrderByDescending(x => x.UpdatedAt);
        }

        private async Task SyncQueuedTrainingExamplesAsync(
            int userId,
            AiTrainingStatusResponse? status)
        {
            if (status == null
                || (status.Status != "succeeded" && status.Status != "failed"))
            {
                return;
            }

            var queuedRows = await _context.ModelTrainingExamples
                .Include(x => x.ChatEvaluation)
                .ThenInclude(x => x!.Conversation)
                .Where(x =>
                    x.IsActive
                    && x.Status == "Training"
                    && (
                        x.Source == "LegacyCsv"
                        || (
                            x.Source == "ReviewWorkspace"
                            && x.ChatEvaluation != null
                            && x.ChatEvaluation.Conversation != null
                            && x.ChatEvaluation.Conversation.UserId == userId
                        )
                    ))
                .ToListAsync();

            if (queuedRows.Count == 0)
            {
                return;
            }

            var nextStatus = status.Status == "succeeded"
                ? "Trained"
                : "Ready";

            foreach (var row in queuedRows)
            {
                row.Status = nextStatus;
                row.UpdatedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        private async Task RecordTrainingRunAsync(
            int userId,
            AiTrainingStatusResponse status,
            int reviewedExampleCount,
            int datasetSize,
            int classCount)
        {
            var now = DateTime.UtcNow;
            var run = new TrainingRun
            {
                UserId = userId,
                Status = status.Status,
                Message = status.Message,
                ReviewedExampleCount = status.ReviewedExampleCount > 0
                    ? status.ReviewedExampleCount
                    : reviewedExampleCount,
                DatasetSize = status.DatasetSize > 0
                    ? status.DatasetSize
                    : datasetSize,
                ClassCount = status.ClassCount > 0
                    ? status.ClassCount
                    : classCount,
                BestModelName = status.BestModelName,
                Accuracy = status.Accuracy,
                ModelVersion = status.ModelVersion,
                Error = status.Error,
                StartedAt = status.StartedAt ?? DateTimeOffset.UtcNow,
                CompletedAt = status.CompletedAt,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.TrainingRuns.Add(run);
            await _context.SaveChangesAsync();
        }

        private async Task SyncTrainingRunsAsync(
            int userId,
            AiTrainingStatusResponse? status)
        {
            if (status == null)
            {
                return;
            }

            var openRun = await _context.TrainingRuns
                .Where(x =>
                    x.CompletedAt == null
                    && (x.UserId == userId || x.UserId == null))
                .OrderByDescending(x => x.CreatedAt)
                .FirstOrDefaultAsync();

            var run = openRun;

            if (run == null && status.StartedAt.HasValue)
            {
                run = await _context.TrainingRuns
                    .Where(x => x.StartedAt == status.StartedAt)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            if (run == null && (status.Status == "succeeded" || status.Status == "failed"))
            {
                run = await _context.TrainingRuns
                    .Where(x => x.UserId == userId || x.UserId == null)
                    .OrderByDescending(x => x.CreatedAt)
                    .FirstOrDefaultAsync();
            }

            if (run == null)
            {
                return;
            }

            run.Status = status.Status;
            run.Message = status.Message;
            run.ReviewedExampleCount = status.ReviewedExampleCount > 0
                ? status.ReviewedExampleCount
                : run.ReviewedExampleCount;
            run.DatasetSize = status.DatasetSize > 0
                ? status.DatasetSize
                : run.DatasetSize;
            run.ClassCount = status.ClassCount > 0
                ? status.ClassCount
                : run.ClassCount;
            run.BestModelName = string.IsNullOrWhiteSpace(status.BestModelName)
                ? run.BestModelName
                : status.BestModelName;
            run.Accuracy = status.Accuracy > 0
                ? status.Accuracy
                : run.Accuracy;
            run.ModelVersion = status.ModelVersion > 0
                ? status.ModelVersion
                : run.ModelVersion;
            run.Error = string.IsNullOrWhiteSpace(status.Error)
                ? run.Error
                : status.Error;
            run.StartedAt ??= status.StartedAt;
            run.CompletedAt = status.CompletedAt ?? run.CompletedAt;
            run.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        private IQueryable<ModelTrainingExample> BuildTrainingDatasetQuery(int userId)
        {
            return _context.ModelTrainingExamples
                .Include(x => x.ChatEvaluation)
                .ThenInclude(x => x!.Conversation)
                .Where(x =>
                    x.IsActive
                    && (
                        x.Source == "LegacyCsv"
                        || (
                            x.Source == "ReviewWorkspace"
                            && x.ChatEvaluation != null
                            && x.ChatEvaluation.Conversation != null
                            && x.ChatEvaluation.Conversation.UserId == userId
                        )
                    ))
                .OrderByDescending(x => x.UpdatedAt);
        }

        private async Task BackfillMissingEvaluationsAsync(int userId)
        {
            var conversationIds = await _context.Conversations
                .Where(x => x.UserId == userId)
                .Select(x => x.Id)
                .ToListAsync();

            if (conversationIds.Count == 0)
            {
                return;
            }

            var existingUserMessageIds = await _context.ChatEvaluations
                .Where(x => conversationIds.Contains(x.ConversationId))
                .Select(x => x.UserMessageId)
                .ToListAsync();

            var existingUserMessageIdSet = existingUserMessageIds.ToHashSet();

            var messages = await _context.Messages
                .Where(x => conversationIds.Contains(x.ConversationId))
                .OrderBy(x => x.ConversationId)
                .ThenBy(x => x.CreatedAt)
                .ThenBy(x => x.Id)
                .ToListAsync();

            var evaluations = new List<ChatEvaluation>();

            foreach (var conversationGroup in messages.GroupBy(x => x.ConversationId))
            {
                Message? pendingUserMessage = null;

                foreach (var message in conversationGroup)
                {
                    if (message.Role.Equals("user", StringComparison.OrdinalIgnoreCase))
                    {
                        pendingUserMessage = message;
                        continue;
                    }

                    if (!message.Role.Equals("assistant", StringComparison.OrdinalIgnoreCase)
                        || pendingUserMessage == null
                        || existingUserMessageIdSet.Contains(pendingUserMessage.Id))
                    {
                        continue;
                    }

                    evaluations.Add(new ChatEvaluation
                    {
                        ConversationId = message.ConversationId,
                        UserMessageId = pendingUserMessage.Id,
                        AssistantMessageId = message.Id,
                        Category = string.IsNullOrWhiteSpace(pendingUserMessage.Category)
                            ? "unknown"
                            : pendingUserMessage.Category,
                        Sentiment = "Neutral",
                        Intent = "BackfilledReview",
                        ConfidenceScore = 50,
                        NeedsHumanReview = true,
                        RetrievedContext = string.Empty,
                        PrimarySourceId = string.Empty,
                        PrimarySourceType = string.Empty,
                        RetrievedSourcesJson = "[]",
                        ImprovementNote = "Backfilled from existing chat history. Review before training.",
                        ApprovedForTraining = false,
                        KnowledgeGap = false,
                        KnowledgeGapResolved = false,
                        HumanCorrectedAnswer = string.Empty,
                        IsDeleted = false,
                        CreatedAt = DateTime.UtcNow
                    });

                    existingUserMessageIdSet.Add(pendingUserMessage.Id);
                    pendingUserMessage = null;
                }
            }

            if (evaluations.Count == 0)
            {
                return;
            }

            _context.ChatEvaluations.AddRange(evaluations);
            await _context.SaveChangesAsync();
        }
    }
}
