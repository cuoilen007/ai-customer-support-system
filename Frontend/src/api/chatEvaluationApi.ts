import axiosClient from "./axiosClient";

export const getChatEvaluations = (
  needsReviewOnly = false
) =>
  axiosClient.get(
    "/chatevaluation",
    {
      params: {
        needsReviewOnly
      }
    }
  );

export const getTrainingData = () =>
  axiosClient.get(
    "/chatevaluation/training-data"
  );

export const getTrainingStatus = () =>
  axiosClient.get(
    "/chatevaluation/training-data/status"
  );

export const runTraining = () =>
  axiosClient.post(
    "/chatevaluation/training-data/run"
  );

export const resolveKnowledgeGaps = (
  evaluationIds: number[]
) =>
  axiosClient.post(
    "/chatevaluation/knowledge-gaps/resolve",
    {
      evaluationIds
    }
  );

export interface UpdateChatEvaluationFeedbackPayload {
  approvedForTraining: boolean;
  knowledgeGap: boolean;
  humanCorrectedAnswer: string;
}

export const updateChatEvaluationFeedback = (
  id: number,
  payload: UpdateChatEvaluationFeedbackPayload
) =>
  axiosClient.put(
    `/chatevaluation/${id}/feedback`,
    payload
  );

export const deleteChatEvaluation = (
  id: number
) =>
  axiosClient.delete(
    `/chatevaluation/${id}`
  );
