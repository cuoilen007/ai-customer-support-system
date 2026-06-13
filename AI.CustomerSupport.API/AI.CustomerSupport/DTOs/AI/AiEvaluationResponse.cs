namespace AI.CustomerSupport.API.DTOs.AI
{
    public class AiEvaluationResponse
    {
        public string Sentiment { get; set; } = "Neutral";

        public string Intent { get; set; } = "GeneralSupport";

        [System.Text.Json.Serialization.JsonPropertyName("confidence_score")]
        public int ConfidenceScore { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("needs_human_review")]
        public bool NeedsHumanReview { get; set; } = true;

        [System.Text.Json.Serialization.JsonPropertyName("improvement_note")]
        public string ImprovementNote { get; set; } = string.Empty;
    }
}
