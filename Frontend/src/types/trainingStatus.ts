export interface TrainingStatus {
  status: string;
  message: string;
  startedAt?: string | null;
  completedAt?: string | null;
  lastUpdatedAt?: string | null;
  reviewedExampleCount: number;
  datasetSize: number;
  classCount: number;
  bestModelName: string;
  accuracy: number;
  modelVersion: number;
  modelPath: string;
  error: string;
}
