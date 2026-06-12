import axiosClient from "./axiosClient";

export const getConversations =
()=>{
  return axiosClient.get(
    "/conversation"
  );
};

export const createConversation =
(
 title:string
)=>{
  return axiosClient.post(
    "/conversation",
    {
      title
    }
  );
};


export const deleteConversation =
  (id: number) =>
{
  return axiosClient.delete(
    `/conversation/${id}`
  );
};