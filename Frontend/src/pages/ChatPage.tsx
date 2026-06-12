import { useState } from "react";

import MainLayout from "../components/layout/MainLayout";
import Sidebar from "../components/layout/Sidebar";
import ChatWindow from "../components/chat/ChatWindow";

export default function ChatPage() {

  const [
    selectedConversation,
    setSelectedConversation
  ] = useState<number | null>(null);

  return (
    <MainLayout>

      <div className="flex h-full">

        <Sidebar
          selectedConversation={
            selectedConversation
          }
          onSelectConversation={
            setSelectedConversation
          }
        />

        <ChatWindow
          conversationId={
            selectedConversation
          }
        />

      </div>

    </MainLayout>
  );
}