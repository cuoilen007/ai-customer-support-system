export default function Header() {

  const logout = () => {

    localStorage.removeItem(
      "token"
    );

    window.location.href =
      "/login";
  };

  return (

    <header
      className="
      h-16
      bg-white
      border-b
      flex
      items-center
      justify-between
      px-6
      "
    >

      <h1
        className="
        text-xl
        font-bold
        "
      >
        AI Customer Support CRM
      </h1>

      <button
        onClick={logout}
        className="
        bg-red-500
        text-white
        px-4
        py-2
        rounded
        "
      >
        Logout
      </button>

    </header>
  );
}