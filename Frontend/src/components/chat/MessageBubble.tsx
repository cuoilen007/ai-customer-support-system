interface MessageBubbleProps {
  role: string;
  content: string;
}

export default function MessageBubble({
  role,
  content,
}: MessageBubbleProps) {

  const isUser =
    role.toLowerCase() === "user";

  return (

    <div
      className={`
      flex mb-4

      ${
        isUser
          ? "justify-end"
          : "justify-start"
      }
      `}
    >

      <div
        className={`
        max-w-[75%]
        px-4
        py-3
        rounded-lg
        shadow

        ${
          isUser
            ? "bg-blue-500 text-white"
            : "bg-white border text-gray-800"
        }
        `}
      >

        <div
          className="
          text-xs
          mb-1
          font-semibold
          opacity-70
          "
        >
          {
            isUser
              ? "You"
              : "AI Assistant"
          }
        </div>

        <div
          className="
          whitespace-pre-wrap
          break-words
          "
        >
          {content}
        </div>

      </div>

    </div>
  );
}