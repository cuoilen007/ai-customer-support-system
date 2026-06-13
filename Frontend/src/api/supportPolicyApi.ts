import axiosClient from "./axiosClient";
import type { SupportPolicyPayload } from "../types/supportPolicy";

export const getSupportPolicies = () =>
  axiosClient.get("/supportpolicy");

export const createSupportPolicy = (
  payload: SupportPolicyPayload
) =>
  axiosClient.post(
    "/supportpolicy",
    payload
  );

export const deleteSupportPolicy = (
  id: number
) =>
  axiosClient.delete(
    `/supportpolicy/${id}`
  );
