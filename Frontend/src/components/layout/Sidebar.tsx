import ConversationList from "../chat/ConversationList";

import {
  Link,
  useLocation,
  useNavigate
} from "react-router-dom";

import {
  ArrowRightOnRectangleIcon,
  BeakerIcon,
  ChartBarIcon,
  ChatBubbleLeftRightIcon,
  CircleStackIcon,
  UserCircleIcon
} from "@heroicons/react/24/outline";

interface Props {
  selectedConversation: number | null;
  onSelectConversation: (id: number | null) => void;
}

export default function Sidebar({
  selectedConversation,
  onSelectConversation
}: Props) {
  const location =
    useLocation();

  const navigate =
    useNavigate();

  const email =
    localStorage.getItem("email");

  const navItems = [
    {
      to: "/chat",
      label: "Conversations",
      icon: ChatBubbleLeftRightIcon
    },
    {
      to: "/knowledge-base",
      label: "Knowledge Base",
      icon: CircleStackIcon
    },
    {
      to: "/ai-review",
      label: "AI Review",
      icon: BeakerIcon
    },
    {
      to: "/dashboard",
      label: "Dashboard",
      icon: ChartBarIcon
    }
  ];

  const handleLogout = () => {
    if (!window.confirm("Logout?")) {
      return;
    }

    localStorage.clear();
    navigate("/login");
  };

  return (
    <aside className="w-80 h-screen bg-slate-900 text-white flex flex-col border-r border-slate-800 shadow-xl">
      <div className="p-5 border-b border-slate-800">
        <h1 className="text-2xl font-bold">
          AI Support CRM
        </h1>
        <p className="text-sm text-slate-400 mt-1">
          Customer Service Platform
        </p>
      </div>

      <div className="p-4 border-b border-slate-800">
        <div className="space-y-2">
          {navItems.map((item) => {
            const Icon = item.icon;
            const active =
              location.pathname === item.to;

            return (
              <Link
                key={item.to}
                to={item.to}
                className={`
                  flex items-center gap-3 px-4 py-3 rounded-lg transition
                  ${active
                    ? "bg-blue-600"
                    : "hover:bg-slate-800"}
                `}
              >
                <Icon className="h-5 w-5 shrink-0" />
                <span className="truncate">
                  {item.label}
                </span>
              </Link>
            );
          })}
        </div>
      </div>

      <div className="flex-1 overflow-y-auto p-4">
        <div className="mb-4">
          <h2 className="text-xs uppercase tracking-widest text-slate-400">
            Conversations
          </h2>
        </div>

        <ConversationList
          selectedConversation={selectedConversation}
          onSelectConversation={onSelectConversation}
        />
      </div>

      <div className="p-4 border-t border-slate-800">
        <div className="flex items-center gap-3 mb-4">
          <div className="w-12 h-12 rounded-full bg-blue-600 flex items-center justify-center">
            <UserCircleIcon className="h-8 w-8" />
          </div>

          <div className="min-w-0">
            <div className="font-medium truncate">
              User
            </div>
            <div className="text-xs text-slate-400 truncate">
              {email}
            </div>
          </div>
        </div>

        <button
          onClick={handleLogout}
          className="w-full py-3 rounded-lg bg-red-500 hover:bg-red-600 text-white font-medium transition"
        >
          <span className="inline-flex items-center justify-center gap-2">
            <ArrowRightOnRectangleIcon className="h-5 w-5" />
            Logout
          </span>
        </button>
      </div>
    </aside>
  );
}
