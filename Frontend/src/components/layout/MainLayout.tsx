import Header from "./Header";

interface Props {
  children: React.ReactNode;
}

export default function MainLayout({
  children
}: Props) {

  return (
    <div className="h-screen flex flex-col">

      <Header />

      <div className="flex-1 overflow-hidden">
        {children}
      </div>

    </div>
  );
}