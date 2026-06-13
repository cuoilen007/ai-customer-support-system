export interface Product {
  id: number;
  name: string;
  description: string;
  category: string;
  price: number;
  status: string;
  createdAt: string;
}

export interface ProductPayload {
  name: string;
  description: string;
  category: string;
  price: number;
  status: string;
}
