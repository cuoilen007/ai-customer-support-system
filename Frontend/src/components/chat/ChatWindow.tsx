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
    async (
      message: string
    ) => {

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
      bg-slate-100
      "
    >

      <div
        className="
        flex-1
        overflow-y-auto
        p-4
        "
      >

        {
          messages.map(
            (message) => (

              <MessageBubble
                key={
                  message.id
                }
                role={
                  message.role
                }
                content={
                  message.content
                }
              />

            )
          )
        }

        <div
          ref={bottomRef}
        />

      </div>

      <MessageInput
        onSend={
          handleSend
        }
      />

    </div>
  );
}