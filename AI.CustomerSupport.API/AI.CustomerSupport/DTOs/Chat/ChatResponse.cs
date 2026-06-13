using AI.CustomerSupport.API.DTOs.AI;

namespace AI.CustomerSupport.API.DTOs.Chat
{
    public class ChatResponse
    {
        public string Answer { get; set; } = string.Empty;

        public ChatEvaluationResponse? Evaluation { get; set; }

        public List<RagSourceResponse> Sources { get; set; } = new();
    }
}
