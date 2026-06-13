export interface DailyConversationCount {
  date: string;
  count: number;
}

export interface LabeledCount {
  label: string;
  count: number;
}

export interface ReviewAnalyticsSummary {
  lowConfidenceCount: number;
  knowledgeGapCount: number;
  readyForTrainingCount: number;
  trainedExampleCount: number;
  totalTrainingRuns: number;
  topReviewIntents: LabeledCount[];
  confidenceBuckets: LabeledCount[];
  reviewOutcomes: LabeledCount[];
}

export interface TrainingRunHistoryItem {
  id: number;
  status: string;
  message: string;
  reviewedExampleCount: number;
  datasetSize: number;
  classCount: number;
  bestModelName: string;
  accuracy: number;
  modelVersion: number;
  error: string;
  startedAt?: string | null;
  completedAt?: string | null;
  updatedAt: string;
}

export interface DashboardAnalyticsResponse {
  totalConversations: number;
  totalMessages: number;
  totalDocuments: number;
  totalProducts: number;
  totalSupportPolicies: number;
  totalChatEvaluations: number;
  totalNeedsReview: number;
  averageConfidenceScore: number;
  messagesByCategory: Record<string, number>;
  weeklyTrends: DailyConversationCount[];
  reviewAnalytics: ReviewAnalyticsSummary;
  trainingRunHistory: TrainingRunHistoryItem[];
}
