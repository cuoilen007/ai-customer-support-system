import {
  useEffect,
  useState
} from "react";

import {
  getConversations
} from "../../api/conversationApi";

interface Props {

  selectedConversation:
    number | null;

  onSelectConversation:
    (id: number) => void;
}

export default function
ConversationList({
  selectedConversation,
  onSelectConversation
}: Props) {

  const [
    conversations,
    setConversations
  ] = useState<any[]>([]);

  useEffect(() => {
    loadData();
  }, []);

  const loadData =
    async () => {

      const res =
        await getConversations();

      setConversations(
        res.data
      );
    };

  return (

    <div>

      {
        conversations.map(
          (conversation) => (

            <div
              key={
                conversation.id
              }

              onClick={() =>
                onSelectConversation(
                  conversation.id
                )
              }

              className={`
              p-3
              rounded
              mb-2
              cursor-pointer

              ${
                selectedConversation ===
                conversation.id
                ? "bg-blue-500"
                : "bg-slate-800 hover:bg-slate-700"
              }
              `}
            >

              {
                conversation.title
              }

            </div>
          )
        )
      }

    </div>
  );
}