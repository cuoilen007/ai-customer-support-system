namespace AI.CustomerSupport.API.Configurations
{
    public class AppCorsOptions
    {
        public const string SectionName = "Cors";

        public List<string> AllowedOrigins { get; set; } = new()
        {
            "http://localhost:5173"
        };
    }
}
