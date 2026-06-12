import ConversationList
from "../chat/ConversationList";

interface Props {

  selectedConversation:
    number | null;

  onSelectConversation:
    (id: number) => void;
}

export default function Sidebar({
  selectedConversation,
  onSelectConversation
}: Props) {

  return (

    <div
      className="
      w-80
      bg-slate-900
      text-white
      p-4
      border-r
      "
    >

      <h2
        className="
        text-xl
        font-bold
        mb-4
        "
      >
        Conversations
      </h2>

      <ConversationList
        selectedConversation={
          selectedConversation
        }
        onSelectConversation={
          onSelectConversation
        }
      />

    </div>
  );
}