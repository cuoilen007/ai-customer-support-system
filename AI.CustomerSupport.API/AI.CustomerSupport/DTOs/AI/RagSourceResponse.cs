namespace AI.CustomerSupport.API.DTOs.AI
{
    public class RagSourceResponse
    {
        [System.Text.Json.Serialization.JsonPropertyName("source_id")]
        public string SourceId { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("source_type")]
        public string SourceType { get; set; } = string.Empty;

        [System.Text.Json.Serialization.JsonPropertyName("relevance_score")]
        public int RelevanceScore { get; set; }

        public string Content { get; set; } = string.Empty;
    }
}
