using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Analytics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AnalyticsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public AnalyticsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetDashboardSummary()
        {
            try
            {
                var totalConversations = await _context.Conversations.CountAsync();
                var totalMessages = await _context.Messages.CountAsync();
                var totalDocuments = await _context.Documents.CountAsync();
                var totalProducts = await _context.Products.CountAsync();
                var totalSupportPolicies = await _context.SupportPolicies.CountAsync();
                var totalChatEvaluations = await _context.ChatEvaluations.CountAsync();
                var totalNeedsReview = await _context.ChatEvaluations.CountAsync(x => x.NeedsHumanReview && !x.IsDeleted);
                var averageConfidenceScore = totalChatEvaluations == 0
                    ? 0
                    : (int)Math.Round(await _context.ChatEvaluations.AverageAsync(x => x.ConfidenceScore));

                var rawMessages = await _context.Messages
                    .Where(m => !string.IsNullOrWhiteSpace(m.Category) && m.Category != "string")
                    .Select(m => m.Category.Trim())
                    .ToListAsync();

                var messagesByCategory = rawMessages
                    .GroupBy(m => char.ToUpper(m[0]) + m.Substring(1).ToLower())
                    .ToDictionary(g => g.Key, g => g.Count());

                var today = DateTime.UtcNow.Date;
                var sevenDaysAgo = today.AddDays(-6);

                var conversationsInWeek = await _context.Conversations
                    .Where(c => c.CreatedAt >= sevenDaysAgo)
                    .ToListAsync();

                var weeklyTrends = Enumerable.Range(0, 7)
                    .Select(offset => sevenDaysAgo.AddDays(offset))
                    .Select(date => new DailyConversationCount
                    {
                        Date = date.ToString("yyyy-MM-dd"),
                        Count = conversationsInWeek.Count(c => c.CreatedAt.Date == date)
                    })
                    .ToList();

                var activeEvaluations = await _context.ChatEvaluations
                    .Where(x => !x.IsDeleted && !x.KnowledgeGapResolved)
                    .ToListAsync();

                var readyForTrainingCount = await _context.ModelTrainingExamples.CountAsync(x =>
                    x.IsActive
                    && x.Source == "ReviewWorkspace"
                    && x.Status == "Ready");

                var trainedExampleCount = await _context.ModelTrainingExamples.CountAsync(x =>
                    x.IsActive
                    && x.Source == "ReviewWorkspace"
                    && x.Status == "Trained");

                var totalTrainingRuns = await _context.TrainingRuns.CountAsync();

                var topReviewIntents = activeEvaluations
                    .Where(x => x.NeedsHumanReview)
                    .GroupBy(x => string.IsNullOrWhiteSpace(x.Intent) ? "Unknown" : x.Intent.Trim())
                    .OrderByDescending(g => g.Count())
                    .Take(5)
                    .Select(g => new LabeledCount
                    {
                        Label = g.Key,
                        Count = g.Count()
                    })
                    .ToList();

                var confidenceBuckets = new List<LabeledCount>
                {
                    new() { Label = "0-39", Count = activeEvaluations.Count(x => x.ConfidenceScore < 40) },
                    new() { Label = "40-69", Count = activeEvaluations.Count(x => x.ConfidenceScore >= 40 && x.ConfidenceScore < 70) },
                    new() { Label = "70-84", Count = activeEvaluations.Count(x => x.ConfidenceScore >= 70 && x.ConfidenceScore < 85) },
                    new() { Label = "85-100", Count = activeEvaluations.Count(x => x.ConfidenceScore >= 85) }
                };

                var reviewOutcomes = new List<LabeledCount>
                {
                    new()
                    {
                        Label = "Pending review",
                        Count = activeEvaluations.Count(x => x.NeedsHumanReview && !x.ApprovedForTraining && !x.KnowledgeGap)
                    },
                    new()
                    {
                        Label = "Knowledge gaps",
                        Count = activeEvaluations.Count(x => x.KnowledgeGap)
                    },
                    new()
                    {
                        Label = "Ready to train",
                        Count = readyForTrainingCount
                    },
                    new()
                    {
                        Label = "Trained",
                        Count = trainedExampleCount
                    }
                };

                var trainingRunHistory = await _context.TrainingRuns
                    .OrderByDescending(x => x.StartedAt ?? DateTimeOffset.MinValue)
                    .ThenByDescending(x => x.UpdatedAt)
                    .Take(10)
                    .Select(x => new TrainingRunHistoryItem
                    {
                        Id = x.Id,
                        Status = x.Status,
                        Message = x.Message,
                        ReviewedExampleCount = x.ReviewedExampleCount,
                        DatasetSize = x.DatasetSize,
                        ClassCount = x.ClassCount,
                        BestModelName = x.BestModelName,
                        Accuracy = x.Accuracy,
                        ModelVersion = x.ModelVersion,
                        Error = x.Error,
                        StartedAt = x.StartedAt,
                        CompletedAt = x.CompletedAt,
                        UpdatedAt = x.UpdatedAt
                    })
                    .ToListAsync();

                var response = new DashboardAnalyticsResponse
                {
                    TotalConversations = totalConversations,
                    TotalMessages = totalMessages,
                    TotalDocuments = totalDocuments,
                    TotalProducts = totalProducts,
                    TotalSupportPolicies = totalSupportPolicies,
                    TotalChatEvaluations = totalChatEvaluations,
                    TotalNeedsReview = totalNeedsReview,
                    AverageConfidenceScore = averageConfidenceScore,
                    MessagesByCategory = messagesByCategory,
                    WeeklyTrends = weeklyTrends,
                    ReviewAnalytics = new ReviewAnalyticsSummary
                    {
                        LowConfidenceCount = activeEvaluations.Count(x => x.ConfidenceScore < 70),
                        KnowledgeGapCount = activeEvaluations.Count(x => x.KnowledgeGap),
                        ReadyForTrainingCount = readyForTrainingCount,
                        TrainedExampleCount = trainedExampleCount,
                        TotalTrainingRuns = totalTrainingRuns,
                        TopReviewIntents = topReviewIntents,
                        ConfidenceBuckets = confidenceBuckets,
                        ReviewOutcomes = reviewOutcomes
                    },
                    TrainingRunHistory = trainingRunHistory
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
