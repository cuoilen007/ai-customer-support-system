namespace AI.CustomerSupport.API.DTOs.Chat
{
    public class ChatEvaluationResponse
    {
        public int Id { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Sentiment { get; set; } = string.Empty;

        public string Intent { get; set; } = string.Empty;

        public int ConfidenceScore { get; set; }

        public bool NeedsHumanReview { get; set; }

        public string ImprovementNote { get; set; } = string.Empty;
    }
}
