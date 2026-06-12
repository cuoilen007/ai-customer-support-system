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
      flex
      mb-6

      ${
        isUser
          ? "justify-end"
          : "justify-start"
      }
      `}
    >

      <div
        className={`
        max-w-[70%]

        ${
          isUser
            ? "order-2"
            : ""
        }
        `}
      >

        <div
          className={`
          text-xs
          mb-1
          px-1

          ${
            isUser
              ? "text-right text-slate-500"
              : "text-slate-500"
          }
          `}
        >
          {
            isUser
              ? "You"
              : "AI Assistant"
          }
        </div>

        <div
          className={`
          px-4
          py-3
          rounded-2xl
          shadow-sm
          whitespace-pre-wrap
          break-words

          ${
            isUser
              ? `
                bg-blue-600
                text-white
                rounded-br-md
              `
              : `
                bg-white
                text-slate-800
                border
                border-slate-200
                rounded-bl-md
              `
          }
          `}
        >
          {content}
        </div>

      </div>

    </div>
  );
}