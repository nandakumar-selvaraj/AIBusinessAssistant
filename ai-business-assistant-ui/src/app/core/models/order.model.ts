export interface OrderItem {
  productId: number;
  productName: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface Order {
  orderId: number;
  orderNumber: string;
  customerId: number;
  customerName: string;
  orderDate: string;
  status: string;
  totalAmount: number;
  orderItems: OrderItem[];
}

export interface CreateOrderItemPayload {
  productId: number;
  quantity: number;
}

/** Body for POST /api/orders. Status, prices and total are set by the API. */
export interface CreateOrderPayload {
  orderNumber: string;
  customerId: number;
  orderItems: CreateOrderItemPayload[];
}
