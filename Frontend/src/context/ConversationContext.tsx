import {
  createContext,
  useContext,
  useState
} from "react";

interface ContextType {

  selectedConversation:
    number | null;

  setSelectedConversation:
    (
      id: number | null
    ) => void;
}

const ConversationContext =
  createContext<ContextType>(
    {} as ContextType
  );

export function
ConversationProvider({
  children
}: {
  children:
    React.ReactNode;
}) {

  const [
    selectedConversation,
    setSelectedConversation
  ] = useState<
    number | null
  >(null);

  return (

    <ConversationContext.Provider
      value={{
        selectedConversation,
        setSelectedConversation
      }}
    >
      {children}
    </ConversationContext.Provider>

  );
}



export const useConversation = () => useContext(ConversationContext);





