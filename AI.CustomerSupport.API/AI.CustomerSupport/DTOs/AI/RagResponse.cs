namespace AI.CustomerSupport.API.DTOs.AI
{
    public class RagResponse
    {
        public string Answer { get; set; }
        = string.Empty;

        public string Context { get; set; }
            = string.Empty;

        public List<RagSourceResponse> Sources { get; set; }
            = new();
    }
}
