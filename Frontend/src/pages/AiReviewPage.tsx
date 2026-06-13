import { useEffect, useMemo, useState } from "react";
import {
  ArrowPathIcon,
  BeakerIcon,
  CheckCircleIcon,
  ExclamationTriangleIcon,
  SparklesIcon
} from "@heroicons/react/24/outline";
import {
  resolveKnowledgeGaps,
  deleteChatEvaluation,
  getChatEvaluations,
  getTrainingStatus,
  getTrainingData,
  runTraining,
  updateChatEvaluationFeedback
} from "../api/chatEvaluationApi";
import { createDocument } from "../api/documentApi";
import { reindexAllKnowledge } from "../api/knowledgeApi";
import { createProduct } from "../api/productApi";
import { createSupportPolicy } from "../api/supportPolicyApi";
import type { ChatEvaluation } from "../types/chatEvaluation";
import type { TrainingExample } from "../types/trainingExample";
import type { TrainingStatus } from "../types/trainingStatus";

type FeedbackDraft = {
  approvedForTraining: boolean;
  knowledgeGap: boolean;
  humanCorrectedAnswer: string;
};

type RowStatus = "idle" | "saved" | "error";
type ReviewView = "review" | "training" | "gaps";

type SimilarReviewGroup = {
  id: string;
  representative: ChatEvaluation;
  items: ChatEvaluation[];
  averageSimilarity: number;
};

type KnowledgeTarget = "documents" | "products" | "policies";

type KnowledgeGapSuggestion = {
  title: string;
  target: KnowledgeTarget;
  content: string;
};

type KnowledgeGapDraft = {
  target: KnowledgeTarget;
  title: string;
  content: string;
  productCategory: string;
  productStatus: string;
  productPrice: string;
  policyType: string;
  effectiveFrom: string;
};

export default function AiReviewPage() {
  const [evaluations, setEvaluations] = useState<ChatEvaluation[]>([]);
  const [trainingExamples, setTrainingExamples] = useState<TrainingExample[]>([]);
  const [loading, setLoading] = useState(false);
  const [savingId, setSavingId] = useState<number | null>(null);
  const [reindexing, setReindexing] = useState(false);
  const [statusMessage, setStatusMessage] = useState("");
  const [feedbackDrafts, setFeedbackDrafts] = useState<Record<number, FeedbackDraft>>({});
  const [rowStatuses, setRowStatuses] = useState<Record<number, RowStatus>>({});
  const [activeView, setActiveView] = useState<ReviewView>("review");
  const [savingGroupId, setSavingGroupId] = useState<string | null>(null);
  const [groupStatuses, setGroupStatuses] = useState<Record<string, RowStatus>>({});
  const [deletingId, setDeletingId] = useState<number | null>(null);
  const [trainingStatus, setTrainingStatus] = useState<TrainingStatus | null>(null);
  const [runningTraining, setRunningTraining] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  useEffect(() => {
    if (trainingStatus?.status !== "running") {
      return;
    }

    const timer = window.setInterval(() => {
      void loadTrainingStatus(false);
    }, 5000);

    return () => window.clearInterval(timer);
  }, [trainingStatus?.status]);

  const uncertainEvaluations = useMemo(
    () => evaluations.filter((item) => isUncertainReview(item) && !item.approvedForTraining),
    [evaluations]
  );

  const similarReviewGroups = useMemo(
    () => buildSimilarReviewGroups(uncertainEvaluations),
    [uncertainEvaluations]
  );

  const knowledgeGapGroups = useMemo(
    () => buildKnowledgeGapGroups(
      uncertainEvaluations.filter((item) => item.knowledgeGap)
    ),
    [uncertainEvaluations]
  );

  const stats = useMemo(() => {
    const total = uncertainEvaluations.length;

    return {
      total,
      approvedCount: trainingExamples.length,
      gapCount: knowledgeGapGroups.length
    };
  }, [knowledgeGapGroups.length, trainingExamples.length, uncertainEvaluations]);

  const groupedReviewIds = useMemo(
    () => new Set(similarReviewGroups.flatMap((group) => group.items.map((item) => item.id))),
    [similarReviewGroups]
  );

  const standaloneReviews = useMemo(
    () => uncertainEvaluations.filter((item) =>
      !item.approvedForTraining
      && !item.knowledgeGap
      && !groupedReviewIds.has(item.id)
    ),
    [groupedReviewIds, uncertainEvaluations]
  );

  const loadTrainingExamples = async () => {
    const trainingRes = await getTrainingData();
    setTrainingExamples(trainingRes.data as TrainingExample[]);
  };

  const loadData = async () => {
    let hasError = false;

    try {
      setLoading(true);
      setStatusMessage("");
      const evaluationRes = await getChatEvaluations(false);

      const nextEvaluations = evaluationRes.data as ChatEvaluation[];
      setEvaluations(nextEvaluations);
      setFeedbackDrafts(buildDraftMap(nextEvaluations));
      setRowStatuses({});
      setGroupStatuses({});

      await loadTrainingExamples();
      await loadTrainingStatus(false);
    } catch (error) {
      console.error(error);
      hasError = true;
      setEvaluations([]);
      setTrainingExamples([]);
      setTrainingStatus(null);
      setFeedbackDrafts({});
      setRowStatuses({});
      setGroupStatuses({});
    }

    setLoading(false);

    if (hasError) {
      setStatusMessage("Some AI Review data could not be loaded. Please check the backend logs or database migrations.");
    }
  };

  const loadTrainingStatus = async (showError = false) => {
    try {
      const response = await getTrainingStatus();
      const nextStatus = response.data as TrainingStatus;
      setTrainingStatus(nextStatus);

      if (nextStatus.status === "succeeded" || nextStatus.status === "failed") {
        await loadTrainingExamples();
      }
    } catch (error) {
      console.error(error);
      if (showError) {
        setStatusMessage("Could not load the training status.");
      }
    }
  };

  const handleDraftChange = (
    id: number,
    patch: Partial<FeedbackDraft>
  ) => {
    setFeedbackDrafts((current) => ({
      ...current,
      [id]: {
        ...current[id],
        ...patch
      }
    }));
    setRowStatuses((current) => ({
      ...current,
      [id]: "idle"
    }));
  };

  const handleSaveFeedback = async (item: ChatEvaluation) => {
    const draft = feedbackDrafts[item.id];

    if (!draft) {
      return;
    }

    try {
      setSavingId(item.id);
      setStatusMessage("");
      setRowStatuses((current) => ({
        ...current,
        [item.id]: "idle"
      }));
      const payload = normalizeFeedbackDraft(draft);

      if (!payload.knowledgeGap && !payload.humanCorrectedAnswer) {
        setStatusMessage(`Please enter a human-corrected answer for chat #${item.id}, or mark it as Needs knowledge base update.`);
        setRowStatuses((current) => ({
          ...current,
          [item.id]: "error"
        }));
        return;
      }

      await updateChatEvaluationFeedback(item.id, payload);
      setEvaluations((current) =>
        current.map((entry) =>
          entry.id === item.id
            ? {
              ...entry,
              approvedForTraining: !payload.knowledgeGap && payload.humanCorrectedAnswer.length > 0,
              knowledgeGap: payload.knowledgeGap,
              humanCorrectedAnswer: payload.humanCorrectedAnswer
            }
            : entry
        )
      );
      setFeedbackDrafts((current) => ({
        ...current,
        [item.id]: payload
      }));

      await loadTrainingExamples();
      setStatusMessage(buildSaveMessage(item.id, payload));
      setRowStatuses((current) => ({
        ...current,
        [item.id]: "saved"
      }));
    } catch (error) {
      console.error(error);
      setStatusMessage(`Could not save feedback for chat #${item.id}.`);
      setRowStatuses((current) => ({
        ...current,
        [item.id]: "error"
      }));
    } finally {
      setSavingId(null);
    }
  };

  const handleSaveGroupFeedback = async (
    group: SimilarReviewGroup,
    humanCorrectedAnswer: string
  ) => {
    const answer = humanCorrectedAnswer.trim();

    if (!answer) {
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "error"
      }));
      setStatusMessage("Please enter a human-corrected answer before submitting the group.");
      return;
    }

    try {
      setSavingGroupId(group.id);
      setStatusMessage("");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "idle"
      }));

      const payload: FeedbackDraft = {
        approvedForTraining: true,
        knowledgeGap: false,
        humanCorrectedAnswer: answer
      };

      for (const item of group.items) {
        await updateChatEvaluationFeedback(item.id, payload);
      }

      const itemIds = new Set(group.items.map((item) => item.id));

      setEvaluations((current) =>
        current.map((entry) =>
          itemIds.has(entry.id)
            ? {
              ...entry,
              approvedForTraining: true,
              knowledgeGap: false,
              humanCorrectedAnswer: answer
            }
            : entry
        )
      );

      setFeedbackDrafts((current) => {
        const next = { ...current };

        for (const item of group.items) {
          next[item.id] = payload;
        }

        return next;
      });

      setRowStatuses((current) => {
        const next = { ...current };

        for (const item of group.items) {
          next[item.id] = "saved";
        }

        return next;
      });

      await loadTrainingExamples();
      setStatusMessage(`Submitted ${group.items.length} reviews to the training set.`);
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "saved"
      }));
    } catch (error) {
      console.error(error);
      setStatusMessage("Could not save group feedback.");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "error"
      }));
    } finally {
      setSavingGroupId(null);
    }
  };

  const handleMarkGroupKnowledgeGap = async (
    group: SimilarReviewGroup
  ) => {
    try {
      setSavingGroupId(group.id);
      setStatusMessage("");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "idle"
      }));

      const payload: FeedbackDraft = {
        approvedForTraining: false,
        knowledgeGap: true,
        humanCorrectedAnswer: ""
      };

      for (const item of group.items) {
        await updateChatEvaluationFeedback(item.id, payload);
      }

      const itemIds = new Set(group.items.map((item) => item.id));

      setEvaluations((current) =>
        current.map((entry) =>
          itemIds.has(entry.id)
            ? {
              ...entry,
              approvedForTraining: false,
              knowledgeGap: true,
              humanCorrectedAnswer: ""
            }
            : entry
        )
      );

      setFeedbackDrafts((current) => {
        const next = { ...current };

        for (const item of group.items) {
          next[item.id] = payload;
        }

        return next;
      });

      setStatusMessage(`Moved ${group.items.length} reviews to Needs knowledge base update.`);
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "saved"
      }));
    } catch (error) {
      console.error(error);
      setStatusMessage("Could not move the group to Needs knowledge base update.");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "error"
      }));
    } finally {
      setSavingGroupId(null);
    }
  };

  const handleDeleteGroup = async (
    group: SimilarReviewGroup
  ) => {
    if (!window.confirm(`Remove all ${group.items.length} reviews in this group from AI Review?`)) {
      return;
    }

    try {
      setSavingGroupId(group.id);
      setStatusMessage("");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "idle"
      }));

      for (const item of group.items) {
        await deleteChatEvaluation(item.id);
      }

      const itemIds = new Set(group.items.map((item) => item.id));

      setEvaluations((current) =>
        current.filter((entry) => !itemIds.has(entry.id))
      );
      setFeedbackDrafts((current) => {
        const next = { ...current };

        for (const item of group.items) {
          delete next[item.id];
        }

        return next;
      });
      setRowStatuses((current) => {
        const next = { ...current };

        for (const item of group.items) {
          delete next[item.id];
        }

        return next;
      });

      await loadTrainingExamples();
      setStatusMessage(`Removed ${group.items.length} reviews from AI Review.`);
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "saved"
      }));
    } catch (error) {
      console.error(error);
      setStatusMessage("Could not remove the group from AI Review.");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "error"
      }));
    } finally {
      setSavingGroupId(null);
    }
  };

  const handleCreateKnowledgeFromGroup = async (
    group: SimilarReviewGroup,
    draft: KnowledgeGapDraft
  ) => {
    try {
      setSavingGroupId(group.id);
      setStatusMessage("");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "idle"
      }));

      if (draft.target === "documents") {
        await createDocument(
          draft.title,
          draft.content
        );
      } else if (draft.target === "products") {
        await createProduct({
          name: draft.title,
          category: draft.productCategory,
          price: Number(draft.productPrice || 0),
          status: draft.productStatus,
          description: draft.content
        });
      } else {
        await createSupportPolicy({
          title: draft.title,
          policyType: draft.policyType,
          effectiveFrom: draft.effectiveFrom,
          content: draft.content
        });
      }

      await resolveKnowledgeGaps(group.items.map((item) => item.id));
      setEvaluations((current) =>
        current.filter((entry) => !group.items.some((item) => item.id === entry.id))
      );
      setStatusMessage(`Created ${formatKnowledgeTargetLabel(draft.target)} content and resolved ${group.items.length} review items.`);
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "saved"
      }));
    } catch (error) {
      console.error(error);
      setStatusMessage("Could not create the suggested knowledge content.");
      setGroupStatuses((current) => ({
        ...current,
        [group.id]: "error"
      }));
    } finally {
      setSavingGroupId(null);
    }
  };

  const handleDeleteEvaluation = async (item: ChatEvaluation) => {
    if (!window.confirm("Remove this review from AI Review?")) {
      return;
    }

    try {
      setDeletingId(item.id);
      setStatusMessage("");
      await deleteChatEvaluation(item.id);

      setEvaluations((current) =>
        current.filter((entry) => entry.id !== item.id)
      );
      setFeedbackDrafts((current) => {
        const next = { ...current };
        delete next[item.id];
        return next;
      });
      setRowStatuses((current) => {
        const next = { ...current };
        delete next[item.id];
        return next;
      });

      await loadTrainingExamples();
      setStatusMessage(`Removed review #${item.id} from AI Review.`);
    } catch (error) {
      console.error(error);
      setStatusMessage(`Could not remove review #${item.id}.`);
    } finally {
      setDeletingId(null);
    }
  };

  const handleReindex = async () => {
    try {
      setReindexing(true);
      setStatusMessage("");
      const response = await reindexAllKnowledge();
      const total = response.data?.total ?? 0;
      setStatusMessage(`Reindexed ${total} records into the AI knowledge base.`);
    } catch (error) {
      console.error(error);
      setStatusMessage("Could not reindex the knowledge base.");
    } finally {
      setReindexing(false);
    }
  };

  const handleRunTraining = async () => {
    if (trainingExamples.length === 0) {
      setStatusMessage("Finalize at least one training example before starting model training.");
      return;
    }

    try {
      setRunningTraining(true);
      setStatusMessage("");
      const response = await runTraining();
      const nextStatus = response.data as TrainingStatus;
      setTrainingStatus(nextStatus);
      await loadTrainingExamples();

      if (nextStatus.status === "running") {
        setStatusMessage("Training started. The page will keep refreshing the training status automatically.");
      } else {
        setStatusMessage(nextStatus.message || "Training request submitted.");
      }
    } catch (error) {
      console.error(error);
      setStatusMessage("Could not start model training.");
    } finally {
      setRunningTraining(false);
    }
  };

  return (
    <div className="h-full overflow-auto bg-slate-100">
      <div className="border-b bg-white px-8 py-5 shadow-sm">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <BeakerIcon className="h-7 w-7 text-blue-600" />
            <div>
              <h1 className="text-2xl font-bold text-slate-800">
                AI Review
              </h1>
              <p className="mt-1 text-sm text-slate-500">
                Review only uncertain AI answers, group similar cases, and prepare corrected examples
              </p>
            </div>
          </div>

          <div className="flex items-center gap-3">
            <button
              onClick={handleReindex}
              disabled={reindexing}
              className="inline-flex items-center gap-2 rounded-md border border-blue-200 bg-blue-50 px-4 py-2 text-sm font-semibold text-blue-700 hover:bg-blue-100 disabled:cursor-not-allowed disabled:opacity-60"
            >
              <SparklesIcon className="h-4 w-4" />
              {reindexing ? "Reindexing..." : "Reindex knowledge"}
            </button>

            <button
              onClick={loadData}
              className="inline-flex items-center gap-2 rounded-md border border-slate-300 px-4 py-2 text-sm font-semibold text-slate-700 hover:bg-slate-50"
            >
              <ArrowPathIcon className="h-4 w-4" />
              Refresh
            </button>
          </div>
        </div>
      </div>

      <div className="mx-auto max-w-6xl p-8">
        {statusMessage && (
          <div className="mb-6 rounded-lg border border-slate-200 bg-white px-4 py-3 text-sm text-slate-700 shadow-sm">
            {statusMessage}
          </div>
        )}

        <div className="mb-6 grid grid-cols-1 gap-4 md:grid-cols-4">
          <MetricCard label="Uncertain reviews" value={stats.total} />
          <MetricCard label="Similar groups" value={similarReviewGroups.length} />
          <MetricCard label="Approved" value={stats.approvedCount} />
          <MetricCard label="Needs knowledge base update" value={stats.gapCount} />
        </div>

        <div className="mb-6 rounded-lg border border-slate-200 bg-white shadow-sm">
          <div className="flex flex-wrap items-center justify-between gap-3 border-b border-slate-200 p-4">
            <div>
              <h2 className="text-base font-bold text-slate-800">
                Uncertain AI reviews
              </h2>
              <p className="text-sm text-slate-500">
                Only low-confidence, needs-review, or missing-knowledge conversations are shown here
              </p>
            </div>
          </div>

          <div className="flex flex-wrap gap-2 border-b border-slate-200 p-4">
            <ReviewTab
              label="Review workspace"
              count={similarReviewGroups.length + standaloneReviews.length}
              active={activeView === "review"}
              onClick={() => setActiveView("review")}
            />
            <ReviewTab
              label="Training set"
              count={stats.approvedCount}
              active={activeView === "training"}
              onClick={() => setActiveView("training")}
            />
            <ReviewTab
              label="Needs knowledge base update"
              count={stats.gapCount}
              active={activeView === "gaps"}
              onClick={() => setActiveView("gaps")}
            />
          </div>

          <div className="divide-y divide-slate-100">
            {loading && (
              <div className="p-8 text-center text-slate-500">
                Loading...
              </div>
            )}

            {!loading && activeView === "review" && similarReviewGroups.length === 0 && standaloneReviews.length === 0 && (
              <div className="p-8 text-center">
                <p className="font-semibold text-slate-700">
                  No review items found
                </p>
                <p className="mt-2 text-sm text-slate-500">
                  No uncertain reviews currently need your attention.
                </p>
              </div>
            )}

            {!loading && activeView === "review" && similarReviewGroups.map((group) => (
              <SimilarReviewGroupPanel
                key={group.id}
                group={group}
                feedbackDrafts={feedbackDrafts}
                savingGroup={savingGroupId === group.id}
                groupStatus={groupStatuses[group.id] ?? "idle"}
                deletingId={deletingId}
                onSaveGroup={handleSaveGroupFeedback}
                onMarkGroupKnowledgeGap={handleMarkGroupKnowledgeGap}
                onDeleteGroup={handleDeleteGroup}
                onDeleteEvaluation={handleDeleteEvaluation}
              />
            ))}

            {!loading && activeView === "review" && standaloneReviews.length > 0 && (
              <div className="border-t border-slate-200 bg-slate-50 px-5 py-3">
                <h3 className="text-sm font-semibold text-slate-800">
                  Standalone reviews
                </h3>
                <p className="mt-1 text-sm text-slate-500">
                  These uncertain reviews do not belong to a similar group yet.
                </p>
              </div>
            )}

            {!loading && activeView === "review" && standaloneReviews.map((item) => (
              <EvaluationRow
                key={item.id}
                item={item}
                draft={feedbackDrafts[item.id]}
                saving={savingId === item.id}
                status={rowStatuses[item.id] ?? "idle"}
                onChange={handleDraftChange}
                onSave={handleSaveFeedback}
              />
            ))}

            {!loading && activeView === "gaps" && knowledgeGapGroups.length === 0 && (
              <div className="p-8 text-center">
                <p className="font-semibold text-slate-700">
                  No knowledge updates needed
                </p>
                <p className="mt-2 text-sm text-slate-500">
                  No grouped knowledge gaps are waiting for action right now.
                </p>
              </div>
            )}

            {!loading && activeView === "gaps" && knowledgeGapGroups.map((group) => (
              <KnowledgeGapGroupPanel
                key={group.id}
                group={group}
                saving={savingGroupId === group.id}
                status={groupStatuses[group.id] ?? "idle"}
                onCreateKnowledge={(draft) => handleCreateKnowledgeFromGroup(group, draft)}
              />
            ))}

            {!loading && activeView === "training" && (
              <div className="p-5">
                <TrainingExamplesPanel
                  trainingExamples={trainingExamples}
                  trainingStatus={trainingStatus}
                  runningTraining={runningTraining}
                  onRunTraining={handleRunTraining}
                />
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

function buildDraftMap(evaluations: ChatEvaluation[]) {
  return evaluations.reduce<Record<number, FeedbackDraft>>((result, item) => {
    result[item.id] = {
      approvedForTraining: item.approvedForTraining,
      knowledgeGap: item.knowledgeGap,
      humanCorrectedAnswer: item.humanCorrectedAnswer ?? ""
    };
    return result;
  }, {});
}

function isUncertainReview(item: ChatEvaluation) {
  return item.needsHumanReview
    || item.confidenceScore < 80
    || item.knowledgeGap;
}

function normalizeFeedbackDraft(draft: FeedbackDraft): FeedbackDraft {
  const humanCorrectedAnswer = draft.humanCorrectedAnswer.trim();

  return {
    approvedForTraining: !draft.knowledgeGap && humanCorrectedAnswer.length > 0,
    knowledgeGap: draft.knowledgeGap,
    humanCorrectedAnswer
  };
}

function buildSaveMessage(
  id: number,
  draft: FeedbackDraft
) {
  if (draft.knowledgeGap) {
    return `Saved chat #${id} to the knowledge base update list.`;
  }

  if (draft.humanCorrectedAnswer) {
    return `Submitted chat #${id} to the training set.`;
  }

  return `Saved feedback for chat #${id}.`;
}

function buildSimilarReviewGroups(
  evaluations: ChatEvaluation[]
): SimilarReviewGroup[] {
  const candidates = evaluations.filter((item) =>
    !item.approvedForTraining
    && !item.knowledgeGap
    && getComparableText(item).length > 0
  );
  const usedIds = new Set<number>();
  const groups: SimilarReviewGroup[] = [];

  for (const candidate of candidates) {
    if (usedIds.has(candidate.id)) {
      continue;
    }

    const candidateTokens = tokenizeReview(candidate);
    const items = [candidate];
    const scores: number[] = [];

    for (const other of candidates) {
      if (other.id === candidate.id || usedIds.has(other.id)) {
        continue;
      }

      const score = getJaccardSimilarity(candidateTokens, tokenizeReview(other));

      if (score >= 0.55) {
        items.push(other);
        scores.push(score);
      }
    }

    if (items.length < 2) {
      continue;
    }

    for (const item of items) {
      usedIds.add(item.id);
    }

    groups.push({
      id: items.map((item) => item.id).join("-"),
      representative: candidate,
      items,
      averageSimilarity: scores.length === 0
        ? 100
        : Math.round(
          scores.reduce((sum, score) => sum + score, 0) / scores.length * 100
        )
    });
  }

  return groups.sort((a, b) =>
    b.items.length - a.items.length
    || b.averageSimilarity - a.averageSimilarity
  );
}

function buildKnowledgeGapGroups(
  evaluations: ChatEvaluation[]
): SimilarReviewGroup[] {
  const candidates = evaluations.filter((item) =>
    getComparableText(item).length > 0
  );
  const usedIds = new Set<number>();
  const groups: SimilarReviewGroup[] = [];

  for (const candidate of candidates) {
    if (usedIds.has(candidate.id)) {
      continue;
    }

    const candidateTokens = tokenizeReview(candidate);
    const items = [candidate];
    const scores: number[] = [];

    for (const other of candidates) {
      if (other.id === candidate.id || usedIds.has(other.id)) {
        continue;
      }

      const score = getJaccardSimilarity(candidateTokens, tokenizeReview(other));

      if (score >= 0.45) {
        items.push(other);
        scores.push(score);
      }
    }

    for (const item of items) {
      usedIds.add(item.id);
    }

    groups.push({
      id: `gap-${items.map((item) => item.id).join("-")}`,
      representative: candidate,
      items,
      averageSimilarity: scores.length === 0
        ? 100
        : Math.round(
          scores.reduce((sum, score) => sum + score, 0) / scores.length * 100
        )
    });
  }

  return groups.sort((a, b) =>
    b.items.length - a.items.length
    || b.averageSimilarity - a.averageSimilarity
  );
}

function buildKnowledgeGapSuggestion(
  items: ChatEvaluation[]
): KnowledgeGapSuggestion {
  const target = inferKnowledgeTarget(items);
  const coreTopic = inferKnowledgeTopic(items);
  const title = target === "products"
    ? `${coreTopic} knowledge entry`
    : target === "policies"
      ? `${coreTopic} policy guidance`
      : `${coreTopic} support note`;

  const questions = getUniqueReviewContents(items)
    .map((entry) => `- ${entry.question}`)
    .join("\n");

  const content = [
    `Suggested knowledge target: ${formatKnowledgeTargetLabel(target)}`,
    "",
    "Summary",
    `This knowledge update should cover ${items.length} related customer questions about ${coreTopic.toLowerCase()}.`,
    "",
    "Customer questions to cover",
    questions,
    "",
    "Suggested answer coverage",
    `Add a clear, reusable explanation for ${coreTopic.toLowerCase()}, including the exact details customers are asking for and any constraints or conditions that support agents should mention.`
  ].join("\n");

  return {
    title,
    target,
    content
  };
}

function buildKnowledgeGapDraft(
  items: ChatEvaluation[]
): KnowledgeGapDraft {
  const suggestion = buildKnowledgeGapSuggestion(items);

  return {
    target: suggestion.target,
    title: suggestion.title,
    content: suggestion.content,
    productCategory: inferProductCategory(items),
    productStatus: "Draft",
    productPrice: "0",
    policyType: inferPolicyType(items),
    effectiveFrom: new Date().toISOString().slice(0, 10)
  };
}

function inferKnowledgeTarget(items: ChatEvaluation[]): KnowledgeTarget {
  const text = normalizeText(items.map(getComparableText).join(" "));

  if (/(refund|return|warranty|shipping|exchange|cancel|policy|delivery|late fee|guarantee)/.test(text)) {
    return "policies";
  }

  if (/(price|stock|color|size|sku|model|product|spec|feature|package|variant|material)/.test(text)) {
    return "products";
  }

  return "documents";
}

function inferKnowledgeTopic(items: ChatEvaluation[]) {
  const firstQuestion = items[0]?.userQuestion?.trim();

  if (!firstQuestion) {
    return "General support";
  }

  return firstQuestion.length > 56
    ? `${firstQuestion.slice(0, 53).trim()}...`
    : firstQuestion;
}

function inferProductCategory(items: ChatEvaluation[]) {
  const category = items.find((item) => item.category)?.category?.trim();
  return category || "General";
}

function inferPolicyType(items: ChatEvaluation[]) {
  const text = normalizeText(items.map(getComparableText).join(" "));

  if (text.includes("refund")) {
    return "Refund";
  }

  if (text.includes("return") || text.includes("exchange")) {
    return "Return";
  }

  if (text.includes("warranty")) {
    return "Warranty";
  }

  if (text.includes("shipping") || text.includes("delivery")) {
    return "Shipping";
  }

  return "Refund";
}

function formatKnowledgeTargetLabel(target: KnowledgeTarget) {
  if (target === "documents") {
    return "Documents";
  }

  if (target === "products") {
    return "Products";
  }

  return "Policies";
}

function getComparableText(item: ChatEvaluation) {
  return [
    item.userQuestion,
    item.humanCorrectedAnswer,
    item.assistantAnswer
  ]
    .filter(Boolean)
    .join(" ");
}

function tokenizeReview(item: ChatEvaluation) {
  const stopWords = new Set([
    "toi",
    "tôi",
    "ban",
    "bạn",
    "cho",
    "hoi",
    "hỏi",
    "co",
    "có",
    "khong",
    "không",
    "la",
    "là",
    "ve",
    "về",
    "cua",
    "của",
    "nao",
    "nào",
    "gi",
    "gì"
  ]);

  return new Set(
    normalizeText(getComparableText(item))
      .split(" ")
      .filter((token) => token.length > 1 && !stopWords.has(token))
  );
}

function normalizeText(value: string) {
  return value
    .toLowerCase()
    .normalize("NFD")
    .replace(/[\u0300-\u036f]/g, "")
    .replace(/đ/g, "d")
    .replace(/[^a-z0-9\s]/g, " ")
    .replace(/\s+/g, " ")
    .trim();
}

function getJaccardSimilarity(
  first: Set<string>,
  second: Set<string>
) {
  if (first.size === 0 || second.size === 0) {
    return 0;
  }

  const intersectionSize = [...first].filter((token) => second.has(token)).length;
  const unionSize = new Set([...first, ...second]).size;

  return intersectionSize / unionSize;
}

function SimilarReviewGroupPanel({
  group,
  feedbackDrafts,
  savingGroup,
  groupStatus,
  deletingId,
  onSaveGroup,
  onMarkGroupKnowledgeGap,
  onDeleteGroup,
  onDeleteEvaluation
}: {
  group: SimilarReviewGroup;
  feedbackDrafts: Record<number, FeedbackDraft>;
  savingGroup: boolean;
  groupStatus: RowStatus;
  deletingId: number | null;
  onSaveGroup: (group: SimilarReviewGroup, humanCorrectedAnswer: string) => void;
  onMarkGroupKnowledgeGap: (group: SimilarReviewGroup) => void;
  onDeleteGroup: (group: SimilarReviewGroup) => void;
  onDeleteEvaluation: (item: ChatEvaluation) => void;
}) {
  const [groupAnswer, setGroupAnswer] = useState(
    group.items.find((item) => feedbackDrafts[item.id]?.humanCorrectedAnswer)
      ? group.items.find((item) => feedbackDrafts[item.id]?.humanCorrectedAnswer)
        ? feedbackDrafts[group.items.find((item) => feedbackDrafts[item.id]?.humanCorrectedAnswer)!.id]?.humanCorrectedAnswer ?? ""
        : ""
      : ""
  );

  return (
    <div className="m-4 overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-100 bg-slate-50 px-5 py-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h3 className="text-sm font-bold text-slate-900">
              Grouped reviews #{group.representative.id}
            </h3>
            <p className="mt-1 text-sm text-slate-500">
              {group.items.length} similar reviews, one corrected answer will apply to the whole group
            </p>
          </div>
          <span className="rounded-md bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">
            {group.averageSimilarity}% similar
          </span>
        </div>
      </div>

      <div className="border-b border-slate-100 p-5">
        <h4 className="mb-3 text-sm font-semibold text-slate-800">
          Review contents in this group
        </h4>

        <div className="space-y-3">
          {getUniqueReviewContents(group.items).map((content, index) => (
            <div
              key={content.id}
              className="rounded-md border border-slate-200 bg-white px-3 py-2"
            >
              <div className="mb-1 flex items-start justify-between gap-3">
                <span className="min-w-0 pr-2 text-xs font-semibold text-slate-500">
                  Review {index + 1} #{content.id}
                </span>
                <button
                  onClick={() => onDeleteEvaluation(content.item)}
                  disabled={deletingId === content.id}
                  className="shrink-0 rounded-md border border-red-200 px-2 py-1 text-xs font-semibold text-red-600 hover:bg-red-50 disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {deletingId === content.id ? "Deleting..." : "Delete"}
                </button>
              </div>
              <p className="break-words text-sm font-medium text-slate-900">
                {content.question}
              </p>
              {content.answer && (
                <p className="mt-2 break-words text-sm text-slate-600">
                  {content.answer}
                </p>
              )}
            </div>
          ))}
        </div>
      </div>

      <div className="grid gap-4 p-5 lg:grid-cols-[1fr_220px]">
        <div>
          <label className="mb-2 block text-sm font-semibold text-slate-800">
            Human corrected answer for this group
          </label>
          <textarea
            value={groupAnswer}
            onChange={(event) => setGroupAnswer(event.target.value)}
            rows={4}
            className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none ring-0 focus:border-blue-400"
            placeholder="Write one corrected answer for all similar reviews in this group."
          />
        </div>

        <div className="flex flex-col gap-3">
          <button
            onClick={() => onSaveGroup(group, groupAnswer)}
            disabled={savingGroup}
            className="mt-auto inline-flex items-center justify-center rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {savingGroup ? "Submitting group..." : "Submit group review"}
          </button>

          <button
            onClick={() => onMarkGroupKnowledgeGap(group)}
            disabled={savingGroup}
            className="inline-flex items-center justify-center rounded-md border border-amber-200 bg-amber-50 px-4 py-2 text-sm font-semibold text-amber-700 hover:bg-amber-100 disabled:cursor-not-allowed disabled:opacity-60"
          >
            Mark group as KB update
          </button>

          <button
            onClick={() => onDeleteGroup(group)}
            disabled={savingGroup}
            className="inline-flex items-center justify-center rounded-md border border-red-200 bg-red-50 px-4 py-2 text-sm font-semibold text-red-600 hover:bg-red-100 disabled:cursor-not-allowed disabled:opacity-60"
          >
            Delete group
          </button>

          {groupStatus === "saved" && (
            <p className="text-xs font-semibold text-emerald-700">
              Group review saved
            </p>
          )}

          {groupStatus === "error" && (
            <p className="text-xs font-semibold text-red-600">
              Group save failed
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

function KnowledgeGapGroupPanel({
  group,
  saving,
  status,
  onCreateKnowledge
}: {
  group: SimilarReviewGroup;
  saving: boolean;
  status: RowStatus;
  onCreateKnowledge: (draft: KnowledgeGapDraft) => void;
}) {
  const initialDraft = useMemo(
    () => buildKnowledgeGapDraft(group.items),
    [group.items]
  );
  const [draft, setDraft] = useState<KnowledgeGapDraft>(initialDraft);

  useEffect(() => {
    setDraft(initialDraft);
  }, [initialDraft]);

  return (
    <div className="m-4 overflow-hidden rounded-lg border border-slate-200 bg-white shadow-sm">
      <div className="border-b border-slate-100 bg-amber-50 px-5 py-4">
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <h3 className="text-sm font-bold text-slate-900">
              Knowledge gap group #{group.representative.id}
            </h3>
            <p className="mt-1 text-sm text-slate-600">
              {group.items.length} related reviews should be added to {formatKnowledgeTargetLabel(draft.target)}.
            </p>
          </div>
          <span className="rounded-md bg-white px-2 py-1 text-xs font-semibold text-amber-700">
            {formatKnowledgeTargetLabel(draft.target)}
          </span>
        </div>
      </div>

      <div className="grid gap-5 p-5 lg:grid-cols-[1fr_280px]">
        <div className="space-y-4">
          <div>
            <p className="text-sm font-semibold text-slate-900">
              Suggested target
            </p>
            <div className="mt-2 flex flex-wrap gap-2">
              {(["documents", "products", "policies"] as KnowledgeTarget[]).map((target) => (
                <button
                  key={target}
                  onClick={() => setDraft((current) => ({ ...current, target }))}
                  className={`rounded-md border px-3 py-2 text-sm font-semibold ${
                    draft.target === target
                      ? "border-slate-900 bg-slate-900 text-white"
                      : "border-slate-200 bg-white text-slate-700 hover:bg-slate-50"
                  }`}
                >
                  {formatKnowledgeTargetLabel(target)}
                </button>
              ))}
            </div>
          </div>

          <div>
            <p className="text-sm font-semibold text-slate-900">
              Title
            </p>
            <input
              value={draft.title}
              onChange={(event) => setDraft((current) => ({ ...current, title: event.target.value }))}
              className="mt-2 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-blue-400"
              placeholder="Knowledge title"
            />
          </div>

          {draft.target === "products" && (
            <div className="grid gap-3 md:grid-cols-3">
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  Category
                </p>
                <input
                  value={draft.productCategory}
                  onChange={(event) => setDraft((current) => ({ ...current, productCategory: event.target.value }))}
                  className="mt-2 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-blue-400"
                  placeholder="Category"
                />
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  Status
                </p>
                <select
                  value={draft.productStatus}
                  onChange={(event) => setDraft((current) => ({ ...current, productStatus: event.target.value }))}
                  className="mt-2 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-blue-400"
                >
                  <option value="Draft">Draft</option>
                  <option value="Active">Active</option>
                  <option value="OutOfStock">Out of stock</option>
                </select>
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  Price
                </p>
                <input
                  type="number"
                  value={draft.productPrice}
                  onChange={(event) => setDraft((current) => ({ ...current, productPrice: event.target.value }))}
                  className="mt-2 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-blue-400"
                  placeholder="0"
                />
              </div>
            </div>
          )}

          {draft.target === "policies" && (
            <div className="grid gap-3 md:grid-cols-2">
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  Policy type
                </p>
                <select
                  value={draft.policyType}
                  onChange={(event) => setDraft((current) => ({ ...current, policyType: event.target.value }))}
                  className="mt-2 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-blue-400"
                >
                  <option value="Refund">Refund</option>
                  <option value="Return">Return</option>
                  <option value="Warranty">Warranty</option>
                  <option value="Shipping">Shipping</option>
                </select>
              </div>
              <div>
                <p className="text-sm font-semibold text-slate-900">
                  Effective from
                </p>
                <input
                  type="date"
                  value={draft.effectiveFrom}
                  onChange={(event) => setDraft((current) => ({ ...current, effectiveFrom: event.target.value }))}
                  className="mt-2 w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none focus:border-blue-400"
                />
              </div>
            </div>
          )}

          <div>
            <p className="text-sm font-semibold text-slate-900">
              Content
            </p>
            <textarea
              value={draft.content}
              onChange={(event) => setDraft((current) => ({ ...current, content: event.target.value }))}
              rows={12}
              className="mt-2 w-full rounded-md border border-slate-300 bg-white px-3 py-3 text-sm text-slate-700 outline-none focus:border-blue-400"
              placeholder="Knowledge content"
            />
          </div>

          <div>
            <p className="text-sm font-semibold text-slate-900">
              Reviews covered by this knowledge update
            </p>
            <div className="mt-2 space-y-2">
              {getUniqueReviewContents(group.items).map((content, index) => (
                <div
                  key={content.id}
                  className="rounded-md border border-slate-200 bg-white px-3 py-2"
                >
                  <div className="mb-1 text-xs font-semibold text-slate-500">
                    Review {index + 1} #{content.id}
                  </div>
                  <p className="break-words text-sm text-slate-800">
                    {content.question}
                  </p>
                </div>
              ))}
            </div>
          </div>
        </div>

        <div className="flex flex-col gap-3">
          <button
            onClick={() => onCreateKnowledge(draft)}
            disabled={saving}
            className="inline-flex items-center justify-center rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {saving ? "Creating knowledge..." : `Create in ${formatKnowledgeTargetLabel(draft.target)}`}
          </button>

          <p className="text-xs text-slate-500">
            When this knowledge entry is created, the grouped reviews will be marked as resolved and removed from this list.
          </p>

          {status === "saved" && (
            <p className="text-xs font-semibold text-emerald-700">
              Knowledge update completed
            </p>
          )}

          {status === "error" && (
            <p className="text-xs font-semibold text-red-600">
              Knowledge update failed
            </p>
          )}
        </div>
      </div>
    </div>
  );
}

function getUniqueReviewContents(items: ChatEvaluation[]) {
  const seen = new Set<string>();
  const contents: Array<{
    id: number;
    item: ChatEvaluation;
    question: string;
    answer: string;
  }> = [];

  for (const item of items) {
    const question = item.userQuestion || "Question not available";
    const answer = item.assistantAnswer || "";
    const key = normalizeText(`${question} ${answer}`);

    if (seen.has(key)) {
      continue;
    }

    seen.add(key);
    contents.push({
      id: item.id,
      item,
      question,
      answer
    });
  }

  return contents;
}

function ReviewTab({
  label,
  count,
  active,
  onClick
}: {
  label: string;
  count: number;
  active: boolean;
  onClick: () => void;
}) {
  return (
    <button
      onClick={onClick}
      className={`rounded-md border px-3 py-2 text-sm font-semibold transition ${
        active
          ? "border-slate-900 bg-slate-900 text-white"
          : "border-slate-200 bg-white text-slate-700 hover:bg-slate-50"
      }`}
    >
      {label}
      <span className={`ml-2 rounded px-1.5 py-0.5 text-xs ${
        active
          ? "bg-white text-slate-900"
          : "bg-slate-100 text-slate-600"
      }`}>
        {count}
      </span>
    </button>
  );
}

function TrainingExamplesPanel({
  trainingExamples,
  trainingStatus,
  runningTraining,
  onRunTraining
}: {
  trainingExamples: TrainingExample[];
  trainingStatus: TrainingStatus | null;
  runningTraining: boolean;
  onRunTraining: () => void;
}) {
  const statusTone = trainingStatus?.status === "failed"
    ? "border-red-200 bg-red-50 text-red-700"
    : trainingStatus?.status === "succeeded"
      ? "border-emerald-200 bg-emerald-50 text-emerald-700"
      : "border-blue-200 bg-blue-50 text-blue-700";
  const groupedExamples = useMemo(
    () => buildTrainingExampleGroups(trainingExamples),
    [trainingExamples]
  );

  return (
    <div className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
        <div className="flex items-center gap-3">
          <CheckCircleIcon className="h-6 w-6 text-emerald-600" />
          <div>
            <h2 className="text-base font-bold text-slate-800">
              Finalized training examples
            </h2>
            <p className="text-sm text-slate-500">
              This list only contains reviews already submitted from Review workspace.
            </p>
          </div>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <StatusBadge
            label={`${trainingExamples.length} waiting`}
            tone="slate"
          />
          <StatusBadge
            label={trainingStatus?.status === "running" ? "Training in progress" : "Training idle"}
            tone={trainingStatus?.status === "running" ? "blue" : "slate"}
          />
          <StatusBadge
            label={trainingStatus?.status === "succeeded" ? "Last run succeeded" : trainingStatus?.status === "failed" ? "Last run failed" : "No completed run"}
            tone={
              trainingStatus?.status === "succeeded"
                ? "green"
                : trainingStatus?.status === "failed"
                  ? "red"
                  : "slate"
            }
          />
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <button
            onClick={onRunTraining}
            disabled={runningTraining || trainingExamples.length === 0 || trainingStatus?.status === "running"}
            className="inline-flex items-center gap-2 rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {runningTraining || trainingStatus?.status === "running"
              ? "Training..."
              : "Run training"}
          </button>
        </div>
      </div>

      <div className="grid gap-4 md:grid-cols-4">
        <div>
          <div className="text-3xl font-bold text-slate-900">
            {trainingExamples.length}
          </div>
          <p className="mt-1 text-sm text-slate-500">
            finalized examples
          </p>
        </div>

        <div>
          <div className="text-3xl font-bold text-slate-900">
            {trainingStatus?.datasetSize ?? 0}
          </div>
          <p className="mt-1 text-sm text-slate-500">
            rows used in the latest training run
          </p>
        </div>

        <div>
          <div className="text-3xl font-bold text-slate-900">
            {trainingStatus?.accuracy
              ? `${Math.round(trainingStatus.accuracy * 100)}%`
              : "--"}
          </div>
          <p className="mt-1 text-sm text-slate-500">
            latest cross-validation accuracy
          </p>
        </div>

        <div>
          <div className="text-3xl font-bold text-slate-900">
            {trainingStatus?.modelVersion
              ? `v${trainingStatus.modelVersion}`
              : "--"}
          </div>
          <p className="mt-1 text-sm text-slate-500">
            current model version
          </p>
        </div>
      </div>

      <div className={`mt-4 rounded-lg border px-4 py-3 text-sm ${statusTone}`}>
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div>
            <p className="font-semibold">
              Training status: {formatTrainingStatus(trainingStatus?.status)}
            </p>
            <p className="mt-1">
              {trainingStatus?.message || "Training has not started yet."}
            </p>
          </div>
          <div className="text-xs">
            {trainingStatus?.completedAt
              ? `Last completed: ${new Date(trainingStatus.completedAt).toLocaleString()}`
              : trainingStatus?.startedAt
                ? `Started: ${new Date(trainingStatus.startedAt).toLocaleString()}`
                : "No training run yet"}
          </div>
        </div>

        {(trainingStatus?.bestModelName
          || trainingStatus?.classCount
          || trainingStatus?.reviewedExampleCount
          || trainingStatus?.completedAt
          || trainingStatus?.modelVersion) && (
          <div className="mt-3 grid gap-3 text-xs md:grid-cols-5">
            <div>
              <span className="font-semibold">Model version:</span> {trainingStatus.modelVersion ? `v${trainingStatus.modelVersion}` : "--"}
            </div>
            <div>
              <span className="font-semibold">Best model:</span> {trainingStatus.bestModelName || "--"}
            </div>
            <div>
              <span className="font-semibold">Classes:</span> {trainingStatus.classCount || 0}
            </div>
            <div>
              <span className="font-semibold">Examples used in latest run:</span> {trainingStatus.reviewedExampleCount || 0}
            </div>
            <div>
              <span className="font-semibold">Last trained:</span> {trainingStatus.completedAt
                ? new Date(trainingStatus.completedAt).toLocaleString()
                : "--"}
            </div>
          </div>
        )}

        {trainingStatus?.error && (
          <p className="mt-3 text-xs font-semibold">
            Error: {trainingStatus.error}
          </p>
        )}
      </div>

      <div className="mt-4 space-y-3">
        {groupedExamples.map((group, groupIndex) => (
          <div
            key={group.key}
            className="rounded-lg border border-slate-200 bg-slate-50 p-4"
          >
            <div className="mb-2 flex flex-wrap items-center gap-2">
              <span className="rounded-md bg-white px-2 py-1 text-xs font-semibold text-slate-600">
                Group #{groupIndex + 1}
              </span>
              <span className="rounded-md bg-white px-2 py-1 text-xs font-semibold text-slate-600">
                {group.items.length} inputs
              </span>
              <span className="rounded-md bg-emerald-100 px-2 py-1 text-xs font-semibold text-emerald-700">
                {group.items[0]?.status ?? "Ready"}
              </span>
            </div>

            <p className="text-sm font-semibold text-slate-900">
              Output
            </p>
            <p className="mt-2 text-sm text-slate-700">
              {group.output}
            </p>

            <div className="mt-4">
              <p className="text-sm font-semibold text-slate-900">
                Inputs
              </p>
              <div className="mt-2 space-y-2">
                {group.items.map((example) => (
                  <div
                    key={example.id}
                    className="rounded-md border border-slate-200 bg-white px-3 py-2"
                  >
                    <div className="mb-1 flex flex-wrap items-center gap-2 text-xs text-slate-500">
                      <span>Example #{example.id}</span>
                      <span>{example.intent}</span>
                    </div>
                    <p className="text-sm text-slate-800">
                      {example.input}
                    </p>
                    {example.originalAnswer && (
                      <p className="mt-2 text-xs text-slate-500">
                        Original: {example.originalAnswer}
                      </p>
                    )}
                  </div>
                ))}
              </div>
            </div>
          </div>
        ))}

        {trainingExamples.length === 0 && (
          <p className="text-sm text-slate-500">
            No training examples have been finalized from the Review workspace yet.
          </p>
        )}
      </div>
    </div>
  );
}

function formatTrainingStatus(status?: string | null) {
  if (!status) {
    return "Idle";
  }

  return status.charAt(0).toUpperCase() + status.slice(1);
}

function StatusBadge({
  label,
  tone
}: {
  label: string;
  tone: "slate" | "blue" | "green" | "red";
}) {
  const toneClass = tone === "blue"
    ? "border-blue-200 bg-blue-50 text-blue-700"
    : tone === "green"
      ? "border-emerald-200 bg-emerald-50 text-emerald-700"
      : tone === "red"
        ? "border-red-200 bg-red-50 text-red-700"
        : "border-slate-200 bg-slate-100 text-slate-700";

  return (
    <span className={`rounded-md border px-2.5 py-1 text-xs font-semibold ${toneClass}`}>
      {label}
    </span>
  );
}

function buildTrainingExampleGroups(trainingExamples: TrainingExample[]) {
  const groups = new Map<string, {
    key: string;
    output: string;
    items: TrainingExample[];
  }>();

  for (const example of trainingExamples) {
    const output = example.output.trim();
    const key = output || `example-${example.id}`;
    const existing = groups.get(key);

    if (existing) {
      existing.items.push(example);
      continue;
    }

    groups.set(key, {
      key,
      output: output || "(No output)",
      items: [example]
    });
  }

  return [...groups.values()].sort((first, second) =>
    second.items.length - first.items.length
    || second.items[0].id - first.items[0].id
  );
}

function MetricCard({
  label,
  value
}: {
  label: string;
  value: string | number;
}) {
  return (
    <div className="rounded-lg border border-slate-200 bg-white p-5 shadow-sm">
      <p className="text-sm text-slate-500">
        {label}
      </p>
      <p className="mt-1 text-3xl font-bold text-slate-900">
        {value}
      </p>
    </div>
  );
}

function EvaluationRow({
  item,
  draft,
  saving,
  status,
  onChange,
  onSave
}: {
  item: ChatEvaluation;
  draft?: FeedbackDraft;
  saving: boolean;
  status: RowStatus;
  onChange: (id: number, patch: Partial<FeedbackDraft>) => void;
  onSave: (item: ChatEvaluation) => void;
}) {
  const scoreColor = item.confidenceScore >= 80
    ? "text-emerald-700 bg-emerald-50"
    : item.confidenceScore >= 60
      ? "text-amber-700 bg-amber-50"
      : "text-red-700 bg-red-50";

  return (
    <div className="p-5">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div className="min-w-0 flex-1">
          <div className="mb-2 flex flex-wrap items-center gap-2">
            <span className={`rounded-md px-2 py-1 text-xs font-bold ${scoreColor}`}>
              {item.confidenceScore}%
            </span>
            <span className="rounded-md bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">
              {item.intent}
            </span>
            <span className="rounded-md bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">
              {item.category}
            </span>
            {item.needsHumanReview && (
              <span className="inline-flex items-center gap-1 rounded-md bg-red-50 px-2 py-1 text-xs font-semibold text-red-700">
                <ExclamationTriangleIcon className="h-3.5 w-3.5" />
                Review
              </span>
            )}
            {item.approvedForTraining && (
              <span className="rounded-md bg-emerald-50 px-2 py-1 text-xs font-semibold text-emerald-700">
                Finalized
              </span>
            )}
            {item.knowledgeGap && (
              <span className="rounded-md bg-amber-50 px-2 py-1 text-xs font-semibold text-amber-700">
                KB update
              </span>
            )}
          </div>

          <p className="text-sm font-semibold text-slate-900">
            {item.userQuestion || "Question not available"}
          </p>
          <p className="mt-2 text-sm text-slate-600">
            {item.assistantAnswer || "Answer not available"}
          </p>
        </div>

        <div className="text-right text-xs text-slate-500">
          {item.createdAt ? new Date(item.createdAt).toLocaleString() : ""}
        </div>
      </div>

      <div className="mt-3 rounded-md bg-slate-50 px-3 py-2 text-sm text-slate-600">
        {item.improvementNote || "No improvement note"}
      </div>

      {(item.primarySourceId || item.primarySourceType) && (
        <div className="mt-3 flex flex-wrap gap-2 text-xs">
          {item.primarySourceType && (
            <span className="rounded-md bg-blue-50 px-2 py-1 font-semibold text-blue-700">
              {item.primarySourceType}
            </span>
          )}
          {item.primarySourceId && (
            <span className="rounded-md bg-slate-100 px-2 py-1 font-medium text-slate-600">
              {item.primarySourceId}
            </span>
          )}
        </div>
      )}

      <div className="mt-4 grid grid-cols-1 gap-4 rounded-lg border border-slate-200 bg-slate-50 p-4 lg:grid-cols-[1fr_220px]">
        <div>
          <label className="mb-2 block text-sm font-semibold text-slate-800">
            Human corrected answer
          </label>
          <textarea
            value={draft?.humanCorrectedAnswer ?? ""}
            onChange={(event) => onChange(item.id, { humanCorrectedAnswer: event.target.value })}
            rows={4}
            className="w-full rounded-md border border-slate-300 bg-white px-3 py-2 text-sm text-slate-700 outline-none ring-0 focus:border-blue-400"
            placeholder="Write the answer you want this chat to become training data for."
          />
        </div>

        <div className="flex flex-col gap-3">
          <label className="inline-flex items-start gap-2 text-sm text-slate-700">
            <input
              type="checkbox"
              checked={draft?.knowledgeGap ?? false}
              onChange={(event) => onChange(item.id, { knowledgeGap: event.target.checked })}
              className="mt-0.5 h-4 w-4 rounded border-slate-300"
            />
            <span>Needs knowledge base update</span>
          </label>

          <button
            onClick={() => onSave(item)}
            disabled={saving}
            className="mt-auto inline-flex items-center justify-center rounded-md bg-slate-900 px-4 py-2 text-sm font-semibold text-white hover:bg-slate-800 disabled:cursor-not-allowed disabled:opacity-60"
          >
            {saving ? "Submitting..." : "Submit review"}
          </button>

          {status === "saved" && (
            <p className="text-xs font-semibold text-emerald-700">
              Review saved
            </p>
          )}

          {status === "error" && (
            <p className="text-xs font-semibold text-red-600">
              Save failed
            </p>
          )}
        </div>
      </div>
    </div>
  );
}
