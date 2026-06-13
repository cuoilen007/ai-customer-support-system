import axiosClient from "./axiosClient";
import type { ChatEvaluation } from "../types/chatEvaluation";
import type { RagSource } from "../types/ragSource";

export interface SendMessageResponse {
  answer: string;
  evaluation?: ChatEvaluation;
  sources?: RagSource[];
}

export const sendMessage =
(
 conversationId:number,
 message:string
)=>{
  return axiosClient.post<SendMessageResponse>(
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
