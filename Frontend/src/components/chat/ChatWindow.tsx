import { useEffect, useRef, useState } from "react";
import { getMessages, sendMessage } from "../../api/chatApi";
import type { ChatEvaluation } from "../../types/chatEvaluation";
import type { RagSource } from "../../types/ragSource";
import MessageBubble from "./MessageBubble";
import MessageInput from "./MessageInput";
import { useConversation } from "../../context/ConversationContext";

interface ChatMessage {
  id: number | string;
  role: string;
  content: string;
}

interface Props {
  conversationId: number | null;
}

export default function ChatWindow({ conversationId }: Props) {
  const [messages, setMessages] = useState<ChatMessage[]>([]);
  const [latestEvaluation, setLatestEvaluation] = useState<ChatEvaluation | null>(null);
  const [latestSources, setLatestSources] = useState<RagSource[]>([]);
  const [sendError, setSendError] = useState("");
  const bottomRef = useRef<HTMLDivElement>(null);
  const pendingMessagesRef = useRef<ChatMessage[]>([]);
  const { setRefreshConversation } = useConversation();

  useEffect(() => {
    if (!conversationId) {
      setMessages([]);
      setLatestEvaluation(null);
      setLatestSources([]);
      setSendError("");
      pendingMessagesRef.current = [];
      return;
    }

    setLatestEvaluation(null);
    setLatestSources([]);
    setSendError("");
    pendingMessagesRef.current = [];
    void loadMessages(conversationId);
  }, [conversationId]);

  useEffect(() => {
    bottomRef.current?.scrollIntoView({
      behavior: "smooth"
    });
  }, [messages]);

  const loadMessages = async (targetConversationId: number) => {
    const res = await getMessages(targetConversationId);
    setMessages([
      ...res.data,
      ...pendingMessagesRef.current
    ]);
  };

  const handleSend = async (message: string) => {
    if (!conversationId) {
      return;
    }

    setSendError("");

    const optimisticUserMessage: ChatMessage = {
      id: `user-${Date.now()}`,
      role: "user",
      content: message
    };

    pendingMessagesRef.current = [
      ...pendingMessagesRef.current,
      optimisticUserMessage
    ];

    setMessages((current) => [...current, optimisticUserMessage]);

    try {
      const res = await sendMessage(conversationId, message);

      setLatestEvaluation(res.data.evaluation ?? null);
      setLatestSources(res.data.sources ?? []);

      pendingMessagesRef.current = pendingMessagesRef.current.filter(
        (item) => item.id !== optimisticUserMessage.id
      );

      setMessages((current) => [
        ...current.filter((item) => item.id !== optimisticUserMessage.id),
        optimisticUserMessage,
        {
          id: `assistant-${Date.now()}`,
          role: "assistant",
          content: res.data.answer
        }
      ]);

      setRefreshConversation((value) => value + 1);

      try {
        await loadMessages(conversationId);
      } catch (error) {
        console.error("Message sync failed", error);
      }
    } catch (error) {
      pendingMessagesRef.current = pendingMessagesRef.current.filter(
        (item) => item.id !== optimisticUserMessage.id
      );
      setMessages((current) =>
        current.filter((item) => item.id !== optimisticUserMessage.id)
      );
      setSendError("Message was not sent. Please try again.");
      throw error;
    }
  };

  if (!conversationId) {
    return (
      <div className="flex-1 flex items-center justify-center text-gray-500">
        Select a conversation
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col h-screen bg-slate-100">
      <div className="bg-white border-b px-6 py-4 shadow-sm shrink-0">
        <h2 className="text-lg font-semibold">
          Customer Support Chat
        </h2>

        <p className="text-sm text-slate-500">
          AI Assistant powered by RAG
        </p>

        {latestEvaluation && (
          <EvaluationStrip evaluation={latestEvaluation} />
        )}

        {latestSources.length > 0 && (
          <SourceStrip sources={latestSources} />
        )}
      </div>

      <div className="flex-1 overflow-y-auto">
        <div className="max-w-4xl mx-auto px-6 py-8">
          {messages.length === 0 && (
            <div className="flex flex-col items-center justify-center h-[60vh] text-center">
              <h2 className="text-3xl font-bold text-slate-700">
                Start Conversation
              </h2>

              <p className="text-slate-500 mt-2">
                Ask anything about products, orders or customer support.
              </p>
            </div>
          )}

          {messages.map((message) => (
            <MessageBubble
              key={message.id}
              role={message.role}
              content={message.content}
            />
          ))}

          <div ref={bottomRef} />
        </div>
      </div>

      <div className="bg-white border-t shrink-0">
        {sendError && (
          <div className="px-6 pt-3 text-sm text-red-600">
            {sendError}
          </div>
        )}
        <MessageInput onSend={handleSend} />
      </div>
    </div>
  );
}

function EvaluationStrip({
  evaluation
}: {
  evaluation: ChatEvaluation;
}) {
  const scoreColor = evaluation.confidenceScore >= 80
    ? "bg-emerald-50 text-emerald-700 border-emerald-200"
    : evaluation.confidenceScore >= 60
      ? "bg-amber-50 text-amber-700 border-amber-200"
      : "bg-red-50 text-red-700 border-red-200";

  return (
    <div className="mt-3 flex flex-wrap items-center gap-2">
      <span className={`rounded-md border px-2 py-1 text-xs font-bold ${scoreColor}`}>
        {evaluation.confidenceScore}% confidence
      </span>
      <span className="rounded-md bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">
        {evaluation.intent}
      </span>
      <span className="rounded-md bg-slate-100 px-2 py-1 text-xs font-semibold text-slate-600">
        {evaluation.category}
      </span>
      {evaluation.needsHumanReview && (
        <span className="rounded-md bg-red-50 px-2 py-1 text-xs font-semibold text-red-700">
          Needs review
        </span>
      )}
      <span className="text-xs text-slate-500">
        {evaluation.improvementNote}
      </span>
    </div>
  );
}

function SourceStrip({
  sources
}: {
  sources: RagSource[];
}) {
  return (
    <div className="mt-3 rounded-lg border border-slate-200 bg-slate-50 p-3">
      <div className="text-xs font-semibold uppercase tracking-wide text-slate-500 mb-2">
        Sources used
      </div>
      <div className="flex flex-wrap gap-2">
        {sources.map((source) => (
          <span
            key={`${source.sourceType}-${source.sourceId}`}
            className="rounded-md bg-white px-2 py-1 text-xs text-slate-700 border border-slate-200"
            title={source.content}
          >
            {source.sourceType} #{source.sourceId} ({source.relevanceScore}%)
          </span>
        ))}
      </div>
    </div>
  );
}
