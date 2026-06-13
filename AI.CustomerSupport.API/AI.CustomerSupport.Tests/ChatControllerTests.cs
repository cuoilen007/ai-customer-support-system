using AI.CustomerSupport.API.Controllers;
using AI.CustomerSupport.API.DTOs.AI;
using AI.CustomerSupport.API.DTOs.Chat;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Xunit;

namespace AI.CustomerSupport.Tests
{
    public class ChatControllerTests
    {
        [Fact]
        public async Task Send_SavesPrimarySourceMetadataInEvaluation()
        {
            using var context = TestDbContextFactory.Create(nameof(Send_SavesPrimarySourceMetadataInEvaluation));

            context.Users.Add(new User
            {
                Id = 7,
                FullName = "Tester",
                Email = "tester@example.com",
                PasswordHash = "hash",
                CreatedAt = DateTime.UtcNow
            });

            context.Conversations.Add(new Conversation
            {
                Id = 12,
                UserId = 7,
                Title = "New Conversation",
                CreatedAt = DateTime.UtcNow
            });

            await context.SaveChangesAsync();

            var aiService = new FakeAiService
            {
                ClassificationResponse = "Product",
                EvaluationResponse = new AiEvaluationResponse
                {
                    Sentiment = "Neutral",
                    Intent = "ProductInfo",
                    ConfidenceScore = 88,
                    NeedsHumanReview = false,
                    ImprovementNote = "Strong answer."
                },
                AskResponse = new RagResponse
                {
                    Answer = "This product includes premium support.",
                    Context = "product context",
                    Sources = new List<RagSourceResponse>
                    {
                        new()
                        {
                            SourceId = "product-2",
                            SourceType = "product",
                            RelevanceScore = 93,
                            Content = "Knowledge type: Product"
                        }
                    }
                }
            };

            var controller = new ChatController(context, aiService)
            {
                ControllerContext = new ControllerContext
                {
                    HttpContext = new DefaultHttpContext
                    {
                        User = new ClaimsPrincipal(
                            new ClaimsIdentity(
                                new[]
                                {
                                    new Claim(ClaimTypes.NameIdentifier, "7")
                                },
                                "test"))
                    }
                }
            };

            var result = await controller.Send(new ChatRequest
            {
                ConversationId = 12,
                Message = "Tell me about the plan"
            });

            Assert.IsType<OkObjectResult>(result);

            var evaluation = context.ChatEvaluations.Single();
            Assert.Equal("product-2", evaluation.PrimarySourceId);
            Assert.Equal("product", evaluation.PrimarySourceType);
            Assert.Contains("product-2", evaluation.RetrievedSourcesJson);
        }
    }
}
