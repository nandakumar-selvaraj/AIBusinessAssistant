export interface Product {
  productId: number;
  name: string;
  sku: string;
  description: string | null;
  unitPrice: number;
  stockQuantity: number;
  isActive: boolean;
  createdAt: string;
}

/** Body for POST /api/products and PUT /api/products/{id}. */
export interface ProductPayload {
  name: string;
  sku: string;
  description: string | null;
  unitPrice: number;
  stockQuantity: number;
  isActive: boolean;
}
