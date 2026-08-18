export interface Product {
  id: string;
  code: string;
  description: string;
  stock: number;
}

export interface CreateProductRequest {
  code: string;
  description: string;
  stock: number;
}
