namespace AI.CustomerSupport.API.Models
{
    public class ChatEvaluation
    {
        public int Id { get; set; }

        public int ConversationId { get; set; }

        public Conversation? Conversation { get; set; }

        public int UserMessageId { get; set; }

        public Message? UserMessage { get; set; }

        public int AssistantMessageId { get; set; }

        public Message? AssistantMessage { get; set; }

        public string Category { get; set; } = string.Empty;

        public string Sentiment { get; set; } = string.Empty;

        public string Intent { get; set; } = string.Empty;

        public int ConfidenceScore { get; set; }

        public bool NeedsHumanReview { get; set; }

        public string RetrievedContext { get; set; } = string.Empty;

        public string PrimarySourceId { get; set; } = string.Empty;

        public string PrimarySourceType { get; set; } = string.Empty;

        public string RetrievedSourcesJson { get; set; } = string.Empty;

        public string ImprovementNote { get; set; } = string.Empty;

        public bool ApprovedForTraining { get; set; }

        public bool KnowledgeGap { get; set; }

        public bool KnowledgeGapResolved { get; set; }

        public string HumanCorrectedAnswer { get; set; } = string.Empty;

        public bool IsDeleted { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
