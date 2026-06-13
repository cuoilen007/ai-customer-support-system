namespace AI.CustomerSupport.API.DTOs.SupportPolicy
{
    public class CreateSupportPolicyRequest
    {
        public string Title { get; set; } = string.Empty;

        public string PolicyType { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public DateTime EffectiveFrom { get; set; }
    }
}
