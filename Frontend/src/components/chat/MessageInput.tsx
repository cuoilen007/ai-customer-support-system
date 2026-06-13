import { useState, type KeyboardEvent } from "react";

interface Props {
  onSend: (message: string) => Promise<void>;
}

export default function MessageInput({ onSend }: Props) {
  const [text, setText] = useState("");
  const [loading, setLoading] = useState(false);

  const handleSend = async () => {
    const message = text.trim();

    if (!message) {
      return;
    }

    try {
      setLoading(true);
      setText("");
      await onSend(message);
    } catch (error) {
      setText(message);
      console.error("Send message failed", error);
    } finally {
      setLoading(false);
    }
  };

  const handleKeyDown = (
    e: KeyboardEvent<HTMLTextAreaElement>
  ) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      void handleSend();
    }
  };

  return (
    <div className="border-t bg-white px-6 py-4">
      <div className="flex gap-3 items-end">
        <textarea
          className="
          flex-1
          border
          border-slate-300
          rounded-xl
          px-4
          py-3
          resize-none
          min-h-[52px]
          max-h-[140px]
          focus:outline-none
          focus:ring-2
          focus:ring-blue-500
          "
          rows={2}
          value={text}
          onChange={(e) => setText(e.target.value)}
          onKeyDown={handleKeyDown}
          placeholder="Type your message..."
        />

        <button
          onClick={() => void handleSend()}
          disabled={loading}
          className="
          bg-blue-600
          hover:bg-blue-700
          text-white
          px-6
          py-3
          rounded-xl
          font-medium
          transition
          disabled:opacity-50
          disabled:cursor-not-allowed
          "
        >
          {loading ? "..." : "Send"}
        </button>
      </div>
    </div>
  );
}
