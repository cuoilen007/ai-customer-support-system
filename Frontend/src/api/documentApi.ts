import axiosClient from "./axiosClient";

export const getDocuments = () =>
  axiosClient.get("/document");

export const createDocument = (
  title: string,
  content: string
) =>
  axiosClient.post(
    "/document",
    {
      title,
      content,
    }
  );

export const deleteDocument = (
  id: number
) =>
  axiosClient.delete(
    `/document/${id}`
  );