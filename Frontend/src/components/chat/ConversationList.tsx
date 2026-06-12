import {
  useEffect,
  useState
} from "react";

import {
  createConversation,
  getConversations,
  deleteConversation
} from "../../api/conversationApi";

interface Props {

  selectedConversation:
    number | null;

  onSelectConversation:
    (
      id: number | null
    ) => void;
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

      try {

        const res =
          await getConversations();

        setConversations(
          res.data
        );

      }
      catch (error) {

        console.error(
          "Load conversations failed",
          error
        );

      }
    };

  const handleCreate =
    async () => {

      try {

        const res =
          await createConversation(
            "New Conversation"
          );

        await loadData();

        onSelectConversation(
          res.data.id
        );

      }
      catch (error) {

        console.error(
          error
        );

      }
    };

  const handleDelete =
    async (
      e: React.MouseEvent,
      id: number
    ) => {

      e.stopPropagation();

      if (
        !window.confirm(
          "Delete conversation?"
        )
      ) {
        return;
      }

      try {

        await deleteConversation(
          id
        );

        await loadData();

        if (
          selectedConversation === id
        ) {

          onSelectConversation(
            null
          );

        }

      }
      catch (error) {

        console.error(
          error
        );

      }
    };

  return (

    <div className="space-y-3">

      <button
        onClick={
          handleCreate
        }
        className="
        w-full
        p-3
        rounded-xl

        bg-blue-600
        hover:bg-blue-700

        text-white
        font-medium

        transition
        "
      >
        + New Chat
      </button>

      {

        conversations.map(
          (
            conversation
          ) => (

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
                p-4
                rounded-xl
                cursor-pointer

                border

                transition-all

                ${
                  selectedConversation ===
                  conversation.id

                    ? `
                      bg-blue-600
                      border-blue-500
                      shadow-lg
                    `

                    : `
                      bg-slate-800
                      border-slate-700
                      hover:bg-slate-700
                    `
                }
              `}
            >

              <div
                className="
                flex
                items-center
                justify-between
                "
              >

                {/* LEFT */}

                <div
                  className="
                  flex
                  items-center
                  gap-3

                  flex-1
                  min-w-0
                  "
                >

                  <div
                    className="
                    w-10
                    h-10

                    rounded-full

                    bg-slate-600

                    flex
                    items-center
                    justify-center

                    text-lg

                    shrink-0
                    "
                  >
                    👤
                  </div>

                  <div
                    className="
                    min-w-0
                    "
                  >

                    <div
                      className="
                      font-semibold
                      truncate
                      "
                    >
                      {
                        conversation.title
                      }
                    </div>

                    <div
                      className="
                      text-xs
                      text-slate-300
                      mt-1
                      "
                    >
                      Customer Support
                    </div>

                  </div>

                </div>

                {/* DELETE */}

                <button
                  onClick={(e) =>
                    handleDelete(
                      e,
                      conversation.id
                    )
                  }

                  className="
                  ml-2

                  text-slate-300
                  hover:text-red-400

                  transition
                  "
                >
                  🗑
                </button>

              </div>

            </div>

          )
        )

      }

      {
        conversations.length === 0 &&

        <div
          className="
          text-center
          text-slate-400
          py-8
          "
        >
          No conversations found
        </div>
      }

    </div>
  );
}