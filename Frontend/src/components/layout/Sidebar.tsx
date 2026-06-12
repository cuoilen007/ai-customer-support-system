import ConversationList
from "../chat/ConversationList";
import {
 Link
}
from "react-router-dom";

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

      <div className="space-y-2">

  <Link
    to="/chat"
    className="
    block
    p-3
    rounded
    hover:bg-slate-700
    "
  >
    Conversations
  </Link>

  <Link
    to="/knowledge-base"
    className="
    block
    p-3
    rounded
    hover:bg-slate-700
    "
  >
    Knowledge Base
  </Link>

</div>

    </div>
  );
}