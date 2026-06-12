import ReactDOM
from "react-dom/client";

import "./index.css";

import App
from "./App";

import {
  ConversationProvider
}
from "./context/ConversationContext";

ReactDOM
.createRoot(
  document.getElementById(
    "root"
  )!
)
.render(

  <ConversationProvider>

    <App />

  </ConversationProvider>

);