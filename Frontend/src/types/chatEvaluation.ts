export interface ChatEvaluation {
  id: number;
  conversationId?: number;
  userQuestion?: string;
  assistantAnswer?: string;
  category: string;
  sentiment: string;
  intent: string;
  confidenceScore: number;
  needsHumanReview: boolean;
  primarySourceId?: string;
  primarySourceType?: string;
  retrievedSourcesJson?: string;
  improvementNote: string;
  approvedForTraining: boolean;
  knowledgeGap: boolean;
  humanCorrectedAnswer: string;
  isDeleted?: boolean;
  createdAt?: string;
}
