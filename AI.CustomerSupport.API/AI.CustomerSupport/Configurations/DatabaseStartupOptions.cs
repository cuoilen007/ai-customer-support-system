namespace AI.CustomerSupport.API.Configurations
{
    public class DatabaseStartupOptions
    {
        public const string SectionName = "Database";

        public bool ApplyMigrationsOnStartup { get; set; } = true;

        public bool SeedOnStartup { get; set; } = true;
    }
}
