namespace AI.CustomerSupport.API.DTOs.Chat
{
    public class ModelTrainingExampleExportItem
    {
        public int Id { get; set; }

        public string Input { get; set; } = string.Empty;

        public string Output { get; set; } = string.Empty;

        public string OriginalAnswer { get; set; } = string.Empty;

        public string Intent { get; set; } = string.Empty;

        public string Status { get; set; } = string.Empty;
    }
}
