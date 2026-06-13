import {
  createContext,
  useContext,
  useState
} from "react";

// 1. Cập nhật Interface để chứa thêm kiểu dữ liệu của refreshConversation
interface ContextType {
  selectedConversation: number | null;
  setSelectedConversation: (id: number | null) => void;
  
  // Thêm kiểu dữ liệu mới ở đây
  refreshConversation: number;
  setRefreshConversation: React.Dispatch<React.SetStateAction<number>>;
}

const ConversationContext =
  createContext<ContextType>(
    {} as ContextType
  );

export function
ConversationProvider({
  children
}: {
  children: React.ReactNode;
}) {

  const [
    selectedConversation,
    setSelectedConversation
  ] = useState<number | null>(null);


  const [
    refreshConversation,
    setRefreshConversation
  ] = useState<number>(0);

  return (
    <ConversationContext.Provider
      value={{
        selectedConversation,
        setSelectedConversation,
        refreshConversation,     
        setRefreshConversation  
      }}
    >
      {children}
    </ConversationContext.Provider>
  );
}

// Khi dùng useConversation() ở chỗ khác, bạn sẽ lấy được cả 4 giá trị trên
export const useConversation = () => useContext(ConversationContext);