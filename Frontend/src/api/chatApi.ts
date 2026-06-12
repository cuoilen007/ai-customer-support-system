import axiosClient from "./axiosClient";

export const sendMessage =
(
 conversationId:number,
 message:string
)=>{
  return axiosClient.post(
    "/chat/send",
    {
        
      conversationId,
      message
    }
  );
};

export const getMessages =
(
 conversationId:number
)=>{
  return axiosClient.get(
    `/message/${conversationId}`
  );
};