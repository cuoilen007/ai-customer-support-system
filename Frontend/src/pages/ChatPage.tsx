import ChatWindow
from "../components/chat/ChatWindow";

import {
  useConversation
}
from "../context/ConversationContext";

export default function
ChatPage() {

  const {
    selectedConversation
  } =
    useConversation();

  return (

    <ChatWindow
      conversationId={
        selectedConversation
      }
    />

  );
}