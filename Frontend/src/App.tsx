import {
 BrowserRouter,
 Routes,
 Route
}
from "react-router-dom";

import LoginPage
from "./pages/LoginPage";

import ChatPage
from "./pages/ChatPage";

import ProtectedRoute
from "./routes/ProtectedRoute";

import KnowledgeBasePage
from "./pages/KnowledgeBasePage";

export default function App(){

 return(

<BrowserRouter>

<Routes>

<Route
 path="/login"
 element={<LoginPage/>}/>

<Route
 path="/"
 element={
<ProtectedRoute>
 <ChatPage/>
</ProtectedRoute>
 }
/>

<Route
  path="/knowledge-base"
  element={
    <KnowledgeBasePage />
  }
/>

</Routes>

</BrowserRouter>

 );
}