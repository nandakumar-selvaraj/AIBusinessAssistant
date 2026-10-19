export interface ChatRequest {
  conversationId?: number;
  message: string;
}

export interface ChatResponse {
  conversationId: number;
  userMessage: string;
  assistantMessage: string;
  createdAt: string;
  orderWorkflow?: OrderWorkflowResponse | null;
}

export interface OrderWorkflowActionRequest {
  action: 'StartPlaceOrder' | 'SelectCustomer' | 'SetOrderItems' | 'EditOrderItems' | 'ConfirmOrder' | 'CancelWorkflow';
  customerId?: number;
  items?: { productId: number; quantity: number }[];
}

export interface OrderWorkflowResponse {
  conversationId: number;
  state: 'Idle' | 'SelectingCustomer' | 'SelectingProducts' | 'AwaitingConfirmation' | 'Completed' | 'Cancelled';
  assistantMessage: string;
  selectedCustomer: { customerId: number; name: string } | null;
  availableCustomers: { customerId: number; name: string }[];
  availableProducts: { productId: number; name: string; sku: string; unitPrice: number; stockQuantity: number }[];
  selectedItems: { productId: number; name: string; quantity: number; unitPrice: number; lineTotal: number; stockAvailable: number; isAvailable: boolean }[];
  orderTotal: number;
  availableActions: string[];
  orderId: number | null;
  orderNumber: string | null;
}
