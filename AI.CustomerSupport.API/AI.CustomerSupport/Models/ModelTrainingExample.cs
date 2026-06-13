namespace AI.CustomerSupport.API.Models
{
    public class ModelTrainingExample
    {
        public int Id { get; set; }

        public int? ChatEvaluationId { get; set; }

        public ChatEvaluation? ChatEvaluation { get; set; }

        public string Input { get; set; } = string.Empty;

        public string ExpectedOutput { get; set; } = string.Empty;

        public string OriginalAnswer { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Intent { get; set; } = string.Empty;

        public string PrimarySourceId { get; set; } = string.Empty;

        public string PrimarySourceType { get; set; } = string.Empty;

        public string Status { get; set; } = "Ready";

        public bool IsActive { get; set; } = true;

        public string Source { get; set; } = "ReviewWorkspace";

        public string SourceReference { get; set; } = string.Empty;

        public DateTime? ImportedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
