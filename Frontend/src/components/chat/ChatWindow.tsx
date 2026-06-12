import {
  useEffect,
  useRef,
  useState
} from "react";

import {
  getMessages,
  sendMessage
} from "../../api/chatApi";

import MessageBubble
from "./MessageBubble";

import MessageInput
from "./MessageInput";



interface Props {
  conversationId:
    number | null;
}

export default function
ChatWindow({
  conversationId
}: Props) {

  const [
    messages,
    setMessages
  ] = useState<any[]>([]);

  const bottomRef =
    useRef<HTMLDivElement>(
      null
    );

  useEffect(() => {

    if (
      conversationId
    ) {
      loadMessages();
    }

  }, [
    conversationId
  ]);

  useEffect(() => {

    bottomRef.current
      ?.scrollIntoView({
        behavior: "smooth"
      });

  }, [messages]);

  const loadMessages =
    async () => {

      const res =
        await getMessages(
          conversationId!
        );

      setMessages(
        res.data
      );
    };

  const handleSend =
    async ( message: string) => {
      await sendMessage(
        conversationId!,
        message
      );

      await loadMessages();
    };



  if (
    !conversationId
  ) {
    return (

      <div
        className="
        flex-1
        flex
        items-center
        justify-center
        text-gray-500
        "
      >
        Select a conversation
      </div>
    );
  }

return (

  <div
    className="
    flex-1
    flex
    flex-col
    h-screen
    bg-slate-100
    "
  >

    {/* Header */}

    <div
      className="
      bg-white
      border-b
      px-6
      py-4
      shadow-sm
      shrink-0
      "
    >
      <h2 className="text-lg font-semibold">
        Customer Support Chat
      </h2>

      <p className="text-sm text-slate-500">
        AI Assistant powered by RAG
      </p>
    </div>

    {/* Message Area */}

    <div
      className="
      flex-1
      overflow-y-auto
      "
    >

      <div
        className="
        max-w-4xl
        mx-auto
        px-6
        py-8
        "
      >

        {
          messages.length === 0 &&

          <div
            className="
            flex
            flex-col
            items-center
            justify-center
            h-[60vh]
            text-center
            "
          >

            <h2
              className="
              text-3xl
              font-bold
              text-slate-700
              "
            >
              Start Conversation
            </h2>

            <p
              className="
              text-slate-500
              mt-2
              "
            >
              Ask anything about products,
              orders or customer support.
            </p>

          </div>
        }

        {
          messages.map(
            (message) => (

              <MessageBubble
                key={message.id}
                role={message.role}
                content={message.content}
              />

            )
          )
        }

        <div ref={bottomRef} />

      </div>

    </div>

    {/* Input */}

    <div
      className="
      bg-white
      border-t
      shrink-0
      "
    >
      <MessageInput
        onSend={handleSend}
      />
    </div>

  </div>
);
}