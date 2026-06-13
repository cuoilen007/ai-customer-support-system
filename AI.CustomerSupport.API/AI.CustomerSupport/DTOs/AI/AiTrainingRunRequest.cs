namespace AI.CustomerSupport.API.DTOs.AI
{
    public class AiTrainingRunRequest
    {
        public List<AiTrainingExampleRequest> Examples { get; set; } = [];
    }
}
