namespace AI.CustomerSupport.API.DTOs.Chat
{
    public class UpdateChatEvaluationFeedbackRequest
    {
        public bool ApprovedForTraining { get; set; }

        public bool KnowledgeGap { get; set; }

        public string? HumanCorrectedAnswer { get; set; }
    }
}
