namespace AI.CustomerSupport.API.Models
{
    public class TrainingRun
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public string Status { get; set; } = "Running";

        public string Message { get; set; } = string.Empty;

        public int ReviewedExampleCount { get; set; }

        public int DatasetSize { get; set; }

        public int ClassCount { get; set; }

        public string BestModelName { get; set; } = string.Empty;

        public double Accuracy { get; set; }

        public int ModelVersion { get; set; }

        public string Error { get; set; } = string.Empty;

        public DateTimeOffset? StartedAt { get; set; }

        public DateTimeOffset? CompletedAt { get; set; }

        public DateTime CreatedAt { get; set; }

        public DateTime UpdatedAt { get; set; }
    }
}
