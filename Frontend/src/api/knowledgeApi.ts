import axiosClient from "./axiosClient";

export const reindexAllKnowledge = () =>
  axiosClient.post("/knowledge/reindex-all");
