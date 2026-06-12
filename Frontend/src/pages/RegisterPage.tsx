import { useState } from "react";
import { register } from "../api/authApi";

export default function RegisterPage() {
  const [email, setEmail] =
    useState("");

  const [password, setPassword] =
    useState("");

  const handleRegister =
    async () => {
      await register(
        email,
        password
      );

      window.location.href =
        "/login";
    };

  return (
    <div
      className="
      flex
      items-center
      justify-center
      h-screen
      "
    >
      <div
        className="
        bg-white
        p-8
        rounded
        shadow
        w-96
        "
      >
        <h2
          className="
          text-2xl
          font-bold
          mb-4
          "
        >
          Register
        </h2>

        <input
          className="
          border
          p-2
          w-full
          mb-3
          "
          placeholder="Email"
          value={email}
          onChange={(e) =>
            setEmail(
              e.target.value
            )
          }
        />

        <input
          type="password"
          className="
          border
          p-2
          w-full
          mb-3
          "
          placeholder="Password"
          value={password}
          onChange={(e) =>
            setPassword(
              e.target.value
            )
          }
        />

        <button
          onClick={
            handleRegister
          }
          className="
          bg-green-500
          text-white
          p-2
          w-full
          rounded
          "
        >
          Register
        </button>
      </div>
    </div>
  );
}