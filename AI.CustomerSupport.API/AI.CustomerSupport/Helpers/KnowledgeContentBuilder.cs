using AI.CustomerSupport.API.Models;

namespace AI.CustomerSupport.API.Helpers
{
    public static class KnowledgeContentBuilder
    {
        public static string GetDocumentVectorId(int id)
        {
            return id.ToString();
        }

        public static string BuildDocumentContent(Document document)
        {
            return $"{document.Title}\n{document.Content}";
        }

        public static string GetProductVectorId(int id)
        {
            return $"product-{id}";
        }

        public static string BuildProductContent(Product product)
        {
            return $"""
            Knowledge type: Product
            Product name: {product.Name}
            Category: {product.Category}
            Price: {product.Price}
            Status: {product.Status}
            Description: {product.Description}
            """;
        }

        public static string GetSupportPolicyVectorId(int id)
        {
            return $"support-policy-{id}";
        }

        public static string BuildSupportPolicyContent(SupportPolicy policy)
        {
            return $"""
            Knowledge type: Support policy
            Title: {policy.Title}
            Policy type: {policy.PolicyType}
            Effective from: {policy.EffectiveFrom:yyyy-MM-dd}
            Content: {policy.Content}
            """;
        }
    }
}
