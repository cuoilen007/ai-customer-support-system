namespace AI.CustomerSupport.API.DTOs.AI
{
    public class AiTrainingStatusResponse
    {
        public string Status { get; set; } = "idle";

        public string Message { get; set; } = string.Empty;

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTimeOffset? LastUpdatedAt { get; set; }

        public int ReviewedExampleCount { get; set; }

        public int DatasetSize { get; set; }

        public int ClassCount { get; set; }

        public string BestModelName { get; set; } = string.Empty;

        public double Accuracy { get; set; }

        public int ModelVersion { get; set; }

        public string ModelPath { get; set; } = string.Empty;

        public string Error { get; set; } = string.Empty;
    }
}
