using AI.CustomerSupport.API.Controllers;
using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.Analytics;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.Tests.TestSupport;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace AI.CustomerSupport.Tests
{
    public class AnalyticsControllerTests
    {
        [Fact]
        public async Task GetDashboardSummary_ReturnsKnowledgeMetrics()
        {
            using var context = TestDbContextFactory.Create(nameof(GetDashboardSummary_ReturnsKnowledgeMetrics));

            context.Documents.Add(new Document
            {
                Title = "Doc",
                Content = "Content",
                CreatedAt = DateTime.UtcNow
            });

            context.Products.Add(new Product
            {
                Name = "Product",
                Description = "Desc",
                Category = "AI",
                Price = 100,
                Status = "Active",
                CreatedAt = DateTime.UtcNow
            });

            context.SupportPolicies.Add(new SupportPolicy
            {
                Title = "Policy",
                PolicyType = "Refund",
                Content = "Policy text",
                EffectiveFrom = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            });

            context.Conversations.Add(new Conversation
            {
                Id = 1,
                UserId = 1,
                Title = "Test",
                CreatedAt = DateTime.UtcNow
            });

            context.Messages.Add(new Message
            {
                ConversationId = 1,
                Role = "user",
                Content = "Need refund",
                Category = "refund",
                CreatedAt = DateTime.UtcNow
            });

            context.ChatEvaluations.Add(new ChatEvaluation
            {
                ConversationId = 1,
                UserMessageId = 1,
                AssistantMessageId = 1,
                Category = "refund",
                Sentiment = "Negative",
                Intent = "ReturnOrRefund",
                ConfidenceScore = 82,
                NeedsHumanReview = false,
                RetrievedContext = "ctx",
                PrimarySourceId = "support-policy-1",
                PrimarySourceType = "support_policy",
                RetrievedSourcesJson = "[]",
                ImprovementNote = "good",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var controller = new AnalyticsController(context);
            var result = await controller.GetDashboardSummary();
            var ok = Assert.IsType<OkObjectResult>(result);
            var payload = Assert.IsType<DashboardAnalyticsResponse>(ok.Value);

            Assert.Equal(1, payload.TotalDocuments);
            Assert.Equal(1, payload.TotalProducts);
            Assert.Equal(1, payload.TotalSupportPolicies);
            Assert.Equal(1, payload.TotalChatEvaluations);
            Assert.Equal(82, payload.AverageConfidenceScore);
        }
    }
}
