namespace AI.CustomerSupport.API.DTOs.AI
{
    public class AiTrainingExampleRequest
    {
        public string Input { get; set; } = string.Empty;

        public string Output { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;

        public string Intent { get; set; } = string.Empty;
    }
}
