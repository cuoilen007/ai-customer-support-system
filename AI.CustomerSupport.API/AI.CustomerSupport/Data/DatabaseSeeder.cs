using AI.CustomerSupport.API.Helpers;
using AI.CustomerSupport.API.Models;
using AI.CustomerSupport.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AI.CustomerSupport.API.Data
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(
            AppDbContext context,
            IAiService aiService)
        {
            await SeedDocumentsAsync(context, aiService);
            await SeedProductsAsync(context, aiService);
            await SeedSupportPoliciesAsync(context, aiService);
        }

        private static async Task SeedDocumentsAsync(
            AppDbContext context,
            IAiService aiService)
        {
            if (await context.Documents.AnyAsync())
            {
                return;
            }

            context.Documents.AddRange(SeedData.Documents);
            await context.SaveChangesAsync();

            foreach (var document in SeedData.Documents)
            {
                await TryIndexAsync(
                    aiService,
                    KnowledgeContentBuilder.GetDocumentVectorId(document.Id),
                    KnowledgeContentBuilder.BuildDocumentContent(document));
            }
        }

        private static async Task SeedProductsAsync(
            AppDbContext context,
            IAiService aiService)
        {
            if (await context.Products.AnyAsync())
            {
                return;
            }

            context.Products.AddRange(SeedData.Products);
            await context.SaveChangesAsync();

            foreach (var product in SeedData.Products)
            {
                await TryIndexAsync(
                    aiService,
                    KnowledgeContentBuilder.GetProductVectorId(product.Id),
                    KnowledgeContentBuilder.BuildProductContent(product));
            }
        }

        private static async Task SeedSupportPoliciesAsync(
            AppDbContext context,
            IAiService aiService)
        {
            if (await context.SupportPolicies.AnyAsync())
            {
                return;
            }

            context.SupportPolicies.AddRange(SeedData.SupportPolicies);
            await context.SaveChangesAsync();

            foreach (var policy in SeedData.SupportPolicies)
            {
                await TryIndexAsync(
                    aiService,
                    KnowledgeContentBuilder.GetSupportPolicyVectorId(policy.Id),
                    KnowledgeContentBuilder.BuildSupportPolicyContent(policy));
            }
        }

        private static async Task TryIndexAsync(
            IAiService aiService,
            string documentId,
            string content)
        {
            try
            {
                await aiService.AddDocumentAsync(documentId, content);
            }
            catch
            {
                Console.WriteLine($"Skipped vector indexing for {documentId}.");
            }
        }
    }
}
