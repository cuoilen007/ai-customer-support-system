import axiosClient from "./axiosClient";

export const login = async (
  email:string,
  password:string
)=>{
  return await axiosClient.post(
    "/auth/login",
    {
      email,
      password
    }
  );
};

export const register = async (
  email:string,
  password:string
)=>{
  return await axiosClient.post(
    "/auth/register",
    {
      email,
      password
    }
  );
};