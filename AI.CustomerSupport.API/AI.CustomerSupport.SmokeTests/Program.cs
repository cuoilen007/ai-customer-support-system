using AI.CustomerSupport.API.Controllers;
using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.API.DTOs.AI;
using AI.CustomerSupport.API.DTOs.Chat;
using AI.CustomerSupport.API.DTOs.Analytics;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

await RunAsync();

static async Task RunAsync()
{
    await ShouldSeedKnowledgeAsync();
    await ShouldReturnAnalyticsSummaryAsync();
    await ShouldSaveSourceMetadataAsync();
    Console.WriteLine("Smoke tests passed.");
}

static AppDbContext CreateContext(string name)
{
    var options = new DbContextOptionsBuilder<AppDbContext>()
        .UseInMemoryDatabase(name)
        .Options;

    return new AppDbContext(options);
}

static async Task ShouldSeedKnowledgeAsync()
{
    using var context = CreateContext(nameof(ShouldSeedKnowledgeAsync));
    var aiService = new FakeAiService();

    await DatabaseSeeder.SeedAsync(context, aiService);

    Ensure(context.Documents.Count() == 3, "Expected 3 documents");
    Ensure(context.Products.Count() == 3, "Expected 3 products");
    Ensure(context.SupportPolicies.Count() == 3, "Expected 3 support policies");
    Ensure(aiService.AddedDocumentCount == 9, "Expected 9 indexed documents");
}

static async Task ShouldReturnAnalyticsSummaryAsync()
{
    using var context = CreateContext(nameof(ShouldReturnAnalyticsSummaryAsync));

    context.Documents.Add(new Document
    {
        Title = "Doc",
        Content = "Body",
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
        Content = "Text",
        EffectiveFrom = DateTime.UtcNow,
        CreatedAt = DateTime.UtcNow
    });
    context.Conversations.Add(new Conversation
    {
        Id = 1,
        UserId = 1,
        Title = "Conversation",
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
        ConfidenceScore = 81,
        NeedsHumanReview = false,
        RetrievedContext = "ctx",
        PrimarySourceId = "support-policy-1",
        PrimarySourceType = "support_policy",
        RetrievedSourcesJson = "[]",
        ImprovementNote = "Good",
        CreatedAt = DateTime.UtcNow
    });
    await context.SaveChangesAsync();

    var controller = new AnalyticsController(context);
    controller.ControllerContext = BuildAuthorizedContext();

    var result = await controller.GetDashboardSummary();
    var ok = result as OkObjectResult;
    var payload = ok?.Value as DashboardAnalyticsResponse;

    Ensure(payload != null, "Expected dashboard payload");
    Ensure(payload.TotalDocuments == 1, "Expected 1 document metric");
    Ensure(payload.TotalProducts == 1, "Expected 1 product metric");
    Ensure(payload.TotalSupportPolicies == 1, "Expected 1 support policy metric");
    Ensure(payload.TotalChatEvaluations == 1, "Expected 1 evaluation metric");
}

static async Task ShouldSaveSourceMetadataAsync()
{
    using var context = CreateContext(nameof(ShouldSaveSourceMetadataAsync));
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
        ControllerContext = BuildAuthorizedContext("7")
    };

    var result = await controller.Send(new ChatRequest
    {
        ConversationId = 12,
        Message = "Tell me about the plan"
    });

    Ensure(result is OkObjectResult, "Expected OK result from chat");
    var evaluation = context.ChatEvaluations.Single();
    Ensure(evaluation.PrimarySourceId == "product-2", "Expected primary source id");
    Ensure(evaluation.PrimarySourceType == "product", "Expected primary source type");
    Ensure(evaluation.RetrievedSourcesJson.Contains("product-2"), "Expected serialized sources");
}

static ControllerContext BuildAuthorizedContext(string userId = "1")
{
    return new ControllerContext
    {
        HttpContext = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        new Claim(ClaimTypes.NameIdentifier, userId)
                    },
                    "test"))
        }
    };
}

static void Ensure(bool condition, string message)
{
    if (!condition)
    {
        throw new InvalidOperationException(message);
    }
}

internal class FakeAiService : IAiService
{
    public int AddedDocumentCount { get; private set; }

    public RagResponse AskResponse { get; set; } = new();

    public AiEvaluationResponse? EvaluationResponse { get; set; }

    public string ClassificationResponse { get; set; } = "General";

    public Task AddDocumentAsync(string documentId, string content)
    {
        AddedDocumentCount++;
        return Task.CompletedTask;
    }

    public Task DeleteDocumentAsync(string documentId)
    {
        return Task.CompletedTask;
    }

    public Task<RagResponse> AskAsync(string question)
    {
        return Task.FromResult(AskResponse);
    }

    public Task<AiEvaluationResponse?> EvaluateAsync(
        string question,
        string answer,
        string context,
        string category)
    {
        return Task.FromResult(EvaluationResponse);
    }

    public Task<bool> IsAiAliveAsync()
    {
        return Task.FromResult(true);
    }

    public Task<string> ClassifyAsync(string text)
    {
        return Task.FromResult(ClassificationResponse);
    }
}
