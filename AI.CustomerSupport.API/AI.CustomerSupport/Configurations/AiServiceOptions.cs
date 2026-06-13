namespace AI.CustomerSupport.API.Configurations
{
    public class AiServiceOptions
    {
        public const string SectionName = "AiService";

        public string BaseUrl { get; set; } = "http://localhost:8000";
    }
}
