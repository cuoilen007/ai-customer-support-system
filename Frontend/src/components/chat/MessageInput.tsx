import {
  useState,
  type KeyboardEvent
} from "react";

interface Props {
  onSend:
    (message: string)
    => Promise<void>;
}

export default function
MessageInput({
  onSend
}: Props) {

  const [
    text,
    setText
  ] = useState("");

  const [
    loading,
    setLoading
  ] = useState(false);

  const handleSend =
    async () => {

      if (
        !text.trim()
      ) {
        return;
      }

      try {

        setLoading(true);

        await onSend(
          text
        );

        setText("");

      } finally {

        setLoading(false);

      }
    };

  const handleKeyDown =
    (
      e: KeyboardEvent<
        HTMLTextAreaElement
      >
    ) => {

      if (
        e.key === "Enter"
        &&
        !e.shiftKey
      ) {

        e.preventDefault();

        handleSend();
      }
    };

  return (

    <div
      className="
      border-t
      bg-white
      p-4
      "
    >

      <div
        className="
        flex
        gap-2
        "
      >

        <textarea
          rows={2}
          value={text}
          onChange={(e) =>
            setText(
              e.target.value
            )
          }
          onKeyDown={
            handleKeyDown
          }
          className="
          flex-1
          border
          rounded
          p-3
          resize-none
          "
        />

        <button
          onClick={
            handleSend
          }
          disabled={
            loading
          }
          className="
          bg-blue-500
          text-white
          px-6
          rounded
          "
        >
          {
            loading
              ? "..."
              : "Send"
          }
        </button>

      </div>

    </div>
  );
}