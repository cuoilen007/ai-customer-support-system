using AI.CustomerSupport.API.Models;

namespace AI.CustomerSupport.API.Data
{
    public static class SeedData
    {
        public static IReadOnlyList<Document> Documents { get; } =
            new List<Document>
            {
                new()
                {
                    Title = "Support onboarding",
                    Content = "When a customer asks about getting started, explain the basic account setup steps, where to find the help center, and how to contact support during business hours.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Escalation rules",
                    Content = "Escalate to a human agent when the customer mentions refund disputes, legal threats, repeated failures, or low confidence answers from the assistant.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Order status guidance",
                    Content = "For order status questions, confirm the order number, explain processing and shipping status, and tell the customer where to track their order in the portal.",
                    CreatedAt = DateTime.UtcNow
                }
            };

        public static IReadOnlyList<Product> Products { get; } =
            new List<Product>
            {
                new()
                {
                    Name = "Support Pro Plan",
                    Category = "Subscription",
                    Price = 490000,
                    Status = "Active",
                    Description = "Priority support package with faster response times, advanced analytics access, and premium assistance for customer support teams.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Name = "Knowledge Base Boost",
                    Category = "AI Feature",
                    Price = 250000,
                    Status = "Active",
                    Description = "Adds knowledge base enrichment, semantic search improvements, and higher quality AI response tracking.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Name = "Chat Review Pack",
                    Category = "Analytics",
                    Price = 180000,
                    Status = "Draft",
                    Description = "Tracks conversation evaluations, training-ready examples, and review queues for support managers.",
                    CreatedAt = DateTime.UtcNow
                }
            };

        public static IReadOnlyList<SupportPolicy> SupportPolicies { get; } =
            new List<SupportPolicy>
            {
                new()
                {
                    Title = "Refund policy",
                    PolicyType = "Refund",
                    EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                    Content = "Refunds are eligible within 7 days for unused services. Escalate unusual cases to a human reviewer.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Return policy",
                    PolicyType = "Return",
                    EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                    Content = "Physical goods can be returned within 14 days if they remain in original condition and packaging.",
                    CreatedAt = DateTime.UtcNow
                },
                new()
                {
                    Title = "Warranty policy",
                    PolicyType = "Warranty",
                    EffectiveFrom = DateTime.UtcNow.AddDays(-30),
                    Content = "Warranty claims require proof of purchase and are handled by the support team within two business days.",
                    CreatedAt = DateTime.UtcNow
                }
            };
    }
}
