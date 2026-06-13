import axiosClient from "./axiosClient";
import type { ProductPayload } from "../types/product";

export const getProducts = () =>
  axiosClient.get("/product");

export const createProduct = (
  payload: ProductPayload
) =>
  axiosClient.post(
    "/product",
    payload
  );

export const deleteProduct = (
  id: number
) =>
  axiosClient.delete(
    `/product/${id}`
  );
