using AI.CustomerSupport.API.Data;
using AI.CustomerSupport.Tests.TestSupport;
using Xunit;

namespace AI.CustomerSupport.Tests
{
    public class DatabaseSeederTests
    {
        [Fact]
        public async Task SeedAsync_AddsKnowledgeRecords_AndIndexesThem()
        {
            using var context = TestDbContextFactory.Create(nameof(SeedAsync_AddsKnowledgeRecords_AndIndexesThem));
            var aiService = new FakeAiService();

            await DatabaseSeeder.SeedAsync(context, aiService);

            Assert.Equal(3, context.Documents.Count());
            Assert.Equal(3, context.Products.Count());
            Assert.Equal(3, context.SupportPolicies.Count());
            Assert.Equal(9, aiService.AddedDocumentCount);
        }
    }
}
