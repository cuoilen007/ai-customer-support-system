using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.AI;
using AI.CustomerSupport.API.DTOs.Chat;
using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IAiService _aiService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            AppDbContext context,
            IAiService aiService,
            ILogger<ChatController> logger)
        {
            _context = context;
            _aiService = aiService;
            _logger = logger;
        }

        [Authorize]
        [HttpPost("send")]
        public async Task<IActionResult> Send(ChatRequest request)
        {
            var userId =
                ClaimsHelper.GetUserId(User);

            var conversation =
                await _context.Conversations
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.ConversationId
                        && x.UserId == userId);

            if (conversation == null)
            {
                return Forbid();
            }

            var category = await TryClassifyAsync(request.Message);

            var userMessage =
                new Message
                {
                    ConversationId = request.ConversationId,
                    Role = "user",
                    Content = request.Message,
                    CreatedAt = DateTime.UtcNow,
                    Category = category
                };

            _context.Messages.Add(userMessage);
            await _context.SaveChangesAsync();

            if (conversation.Title == "New Conversation")
            {
                conversation.Title =
                    request.Message.Length > 40
                        ? request.Message.Substring(0, 40)
                        : request.Message;

                await _context.SaveChangesAsync();
            }

            var answer = await TryAskAsync(request.Message);

            var assistantMessage =
                new Message
                {
                    ConversationId = request.ConversationId,
                    Role = "assistant",
                    Content = answer.Answer,
                    CreatedAt = DateTime.UtcNow
                };

            _context.Messages.Add(assistantMessage);
            await _context.SaveChangesAsync();

            var aiEvaluation = await _aiService.EvaluateAsync(
                request.Message,
                answer.Answer,
                answer.Context,
                category
            );

            var evaluation = aiEvaluation == null
                ? BuildFallbackEvaluation(
                    request.ConversationId,
                    userMessage,
                    assistantMessage,
                    category,
                    request.Message,
                    answer.Answer,
                    answer.Context,
                    answer.Sources
                )
                : BuildEvaluationFromAi(
                    request.ConversationId,
                    userMessage,
                    assistantMessage,
                    category,
                    answer.Context,
                    answer.Sources,
                    aiEvaluation
                );

            var evaluationSaved = await TrySaveEvaluationAsync(evaluation);

            return Ok(
                new ChatResponse
                {
                    Answer = answer.Answer,
                    Sources = answer.Sources.Select(x => new RagSourceResponse
                    {
                        SourceId = x.SourceId,
                        SourceType = x.SourceType,
                        RelevanceScore = x.RelevanceScore,
                        Content = x.Content
                    }).ToList(),
                    Evaluation = new ChatEvaluationResponse
                    {
                        Id = evaluationSaved ? evaluation.Id : 0,
                        Category = evaluation.Category,
                        Sentiment = evaluation.Sentiment,
                        Intent = evaluation.Intent,
                        ConfidenceScore = evaluation.ConfidenceScore,
                        NeedsHumanReview = evaluation.NeedsHumanReview,
                        ImprovementNote = evaluation.ImprovementNote
                    }
                });
        }

        private async Task<string> TryClassifyAsync(string message)
        {
            try
            {
                return await _aiService.ClassifyAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "AI classification failed. Falling back to unknown category.");

                return "unknown";
            }
        }

        private async Task<RagResponse> TryAskAsync(string message)
        {
            try
            {
                return await _aiService.AskAsync(message);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "AI answer generation failed. Returning fallback answer.");

                return new RagResponse
                {
                    Answer = "AI service is temporarily unavailable. Please try again in a moment.",
                    Context = string.Empty,
                    Sources = new List<RagSourceResponse>()
                };
            }
        }

        private async Task<bool> TrySaveEvaluationAsync(
            ChatEvaluation evaluation)
        {
            try
            {
                _context.ChatEvaluations.Add(evaluation);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(
                    ex,
                    "Could not save chat evaluation. Returning chat response without persisted evaluation.");

                return false;
            }
        }

        private static ChatEvaluation BuildEvaluationFromAi(
            int conversationId,
            Message userMessage,
            Message assistantMessage,
            string category,
            string context,
            List<RagSourceResponse> sources,
            AiEvaluationResponse aiEvaluation)
        {
            var primarySource = sources.FirstOrDefault();

            return new ChatEvaluation
            {
                ConversationId = conversationId,
                UserMessageId = userMessage.Id,
                AssistantMessageId = assistantMessage.Id,
                Category = category,
                Sentiment = aiEvaluation.Sentiment,
                Intent = aiEvaluation.Intent,
                ConfidenceScore = Math.Clamp(
                    aiEvaluation.ConfidenceScore,
                    0,
                    100
                ),
                NeedsHumanReview = aiEvaluation.NeedsHumanReview,
                RetrievedContext = context,
                PrimarySourceId = primarySource?.SourceId ?? string.Empty,
                PrimarySourceType = primarySource?.SourceType ?? string.Empty,
                RetrievedSourcesJson = SerializeSources(sources),
                ImprovementNote = aiEvaluation.ImprovementNote,
                CreatedAt = DateTime.UtcNow
            };
        }

        private static ChatEvaluation BuildFallbackEvaluation(
            int conversationId,
            Message userMessage,
            Message assistantMessage,
            string category,
            string question,
            string answer,
            string context,
            List<RagSourceResponse> sources)
        {
            var hasContext = !string.IsNullOrWhiteSpace(context);
            var isFallbackAnswer = answer.Contains(
                "khong tim thay",
                StringComparison.OrdinalIgnoreCase
            ) || answer.Contains(
                "không tìm thấy",
                StringComparison.OrdinalIgnoreCase
            );

            var sentiment = DetectSentiment(question);
            var intent = DetectIntent(question);
            var confidenceScore = 45;

            if (hasContext)
            {
                confidenceScore += 30;
            }

            if (!isFallbackAnswer)
            {
                confidenceScore += 15;
            }

            if (category != "unknown")
            {
                confidenceScore += 10;
            }

            confidenceScore = Math.Clamp(confidenceScore, 0, 100);
            var primarySource = sources.FirstOrDefault();
            var needsHumanReview =
                confidenceScore < 70
                || isFallbackAnswer
                || sentiment == "Negative";

            return new ChatEvaluation
            {
                ConversationId = conversationId,
                UserMessageId = userMessage.Id,
                AssistantMessageId = assistantMessage.Id,
                Category = category,
                Sentiment = sentiment,
                Intent = intent,
                ConfidenceScore = confidenceScore,
                NeedsHumanReview = needsHumanReview,
                RetrievedContext = context,
                PrimarySourceId = primarySource?.SourceId ?? string.Empty,
                PrimarySourceType = primarySource?.SourceType ?? string.Empty,
                RetrievedSourcesJson = SerializeSources(sources),
                ImprovementNote = BuildImprovementNote(
                    hasContext,
                    isFallbackAnswer,
                    needsHumanReview
                ),
                CreatedAt = DateTime.UtcNow
            };
        }

        private static string DetectSentiment(string text)
        {
            var negativeWords = new[]
            {
                "te",
                "tệ",
                "loi",
                "lỗi",
                "buc",
                "bực",
                "khong hai long",
                "không hài lòng",
                "cham",
                "chậm",
                "hong",
                "hỏng",
                "khieu nai",
                "khiếu nại"
            };

            return negativeWords.Any(x =>
                text.Contains(x, StringComparison.OrdinalIgnoreCase)
            )
                ? "Negative"
                : "Neutral";
        }

        private static string DetectIntent(string text)
        {
            if (text.Contains("gia", StringComparison.OrdinalIgnoreCase)
                || text.Contains("giá", StringComparison.OrdinalIgnoreCase)
                || text.Contains("bao nhieu", StringComparison.OrdinalIgnoreCase)
                || text.Contains("bao nhiêu", StringComparison.OrdinalIgnoreCase))
            {
                return "Pricing";
            }

            if (text.Contains("doi tra", StringComparison.OrdinalIgnoreCase)
                || text.Contains("đổi trả", StringComparison.OrdinalIgnoreCase)
                || text.Contains("hoan tien", StringComparison.OrdinalIgnoreCase)
                || text.Contains("hoàn tiền", StringComparison.OrdinalIgnoreCase))
            {
                return "ReturnOrRefund";
            }

            if (text.Contains("bao hanh", StringComparison.OrdinalIgnoreCase)
                || text.Contains("bảo hành", StringComparison.OrdinalIgnoreCase))
            {
                return "Warranty";
            }

            if (text.Contains("san pham", StringComparison.OrdinalIgnoreCase)
                || text.Contains("sản phẩm", StringComparison.OrdinalIgnoreCase))
            {
                return "ProductInfo";
            }

            return "GeneralSupport";
        }

        private static string BuildImprovementNote(
            bool hasContext,
            bool isFallbackAnswer,
            bool needsHumanReview)
        {
            if (!hasContext)
            {
                return "Add or improve knowledge base records for this question.";
            }

            if (isFallbackAnswer)
            {
                return "Retrieved context did not answer the user clearly; review wording and coverage.";
            }

            if (needsHumanReview)
            {
                return "Review this exchange and consider adding it to training data.";
            }

            return "Good candidate for positive training examples.";
        }

        private static string SerializeSources(
            List<RagSourceResponse> sources)
        {
            return JsonSerializer.Serialize(
                sources.Select(x => new
                {
                    x.SourceId,
                    x.SourceType,
                    x.RelevanceScore
                }));
        }
    }
}
