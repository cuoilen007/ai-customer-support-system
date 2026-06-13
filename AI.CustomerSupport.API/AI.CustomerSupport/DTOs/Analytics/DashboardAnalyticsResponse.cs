namespace AI.CustomerSupport.API.DTOs.Analytics
{
    public class DashboardAnalyticsResponse
    {
        public int TotalConversations { get; set; }
        public int TotalMessages { get; set; }
        public int TotalDocuments { get; set; }
        public int TotalProducts { get; set; }
        public int TotalSupportPolicies { get; set; }
        public int TotalChatEvaluations { get; set; }
        public int TotalNeedsReview { get; set; }
        public int AverageConfidenceScore { get; set; }

        // Dữ liệu cho biểu đồ cột (Gom nhóm Category từ Message)
        public Dictionary<string, int> MessagesByCategory { get; set; } = new();

        public List<DailyConversationCount> WeeklyTrends { get; set; } = new();

        public ReviewAnalyticsSummary ReviewAnalytics { get; set; } = new();

        public List<TrainingRunHistoryItem> TrainingRunHistory { get; set; } = new();
    }

    public class DailyConversationCount
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ReviewAnalyticsSummary
    {
        public int LowConfidenceCount { get; set; }
        public int KnowledgeGapCount { get; set; }
        public int ReadyForTrainingCount { get; set; }
        public int TrainedExampleCount { get; set; }
        public int TotalTrainingRuns { get; set; }
        public List<LabeledCount> TopReviewIntents { get; set; } = new();
        public List<LabeledCount> ConfidenceBuckets { get; set; } = new();
        public List<LabeledCount> ReviewOutcomes { get; set; } = new();
    }

    public class LabeledCount
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class TrainingRunHistoryItem
    {
        public int Id { get; set; }
        public string Status { get; set; } = string.Empty;
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
        public DateTime UpdatedAt { get; set; }
    }
}
