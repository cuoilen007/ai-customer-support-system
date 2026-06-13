namespace AI.CustomerSupport.API.Models
{
    public class SupportPolicy
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string PolicyType { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime EffectiveFrom { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
