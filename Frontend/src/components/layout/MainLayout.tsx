import { Outlet }
from "react-router-dom";

import Sidebar
from "./Sidebar";

import {
  useConversation
}
from "../../context/ConversationContext";

export default function
MainLayout() {

  const {

    selectedConversation,

    setSelectedConversation

  } =
    useConversation();

  return (

    <div
      className="
      flex
      h-screen
      "
    >

      <Sidebar
        selectedConversation={
          selectedConversation
        }
        onSelectConversation={
          setSelectedConversation
        }
      />

      <main
        className="
        flex-1
        overflow-hidden
        "
      >
        <Outlet />
      </main>

    </div>
  );
}