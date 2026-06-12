import { useState }
    from "react";

import {
    login
}
    from "../api/authApi";

export default function LoginPage() {

    const [email, setEmail]
        = useState("");

    const [password,
        setPassword]
        = useState("");

    const handleLogin =
        async () => {

            const res =
                await login(
                    email,
                    password
                );

            localStorage.setItem(
                "token",
                res.data.token
            );

            localStorage.setItem(
                "email",
                email
            );

            window.location.href = "/chat";
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
                        mb-4"
                >
                    Login
                </h2>

                <input
                    className="
border
w-full
p-2
mb-3
"
                    placeholder="Email"
                    value={email}
                    onChange={
                        e => setEmail(
                            e.target.value
                        )
                    }
                />

                <input
                    type="password"
                    className="
border
w-full
p-2
mb-3
"
                    placeholder="Password"
                    value={password}
                    onChange={
                        e => setPassword(
                            e.target.value
                        )
                    }
                />

                <button
                    onClick={
                        handleLogin
                    }
                    className="
bg-blue-500
text-white
w-full
p-2
"
                >
                    Login
                </button>

            </div>

        </div>

    );
}