import {
 createContext,
 useState
} from "react";

export const AuthContext =
createContext<any>(null);

export const AuthProvider =
({children}:any)=>{

 const [token,setToken] =
 useState(
   localStorage.getItem(
     "token"
   )
 );

 const login=(jwt:string)=>{

   localStorage.setItem(
     "token",
     jwt
   );

   setToken(jwt);
 };

 const logout=()=>{

   localStorage.removeItem(
     "token"
   );

   setToken(null);
 };

 return(

<AuthContext.Provider
 value={{
  token,
  login,
  logout
 }}
>

{children}

</AuthContext.Provider>

 );
};