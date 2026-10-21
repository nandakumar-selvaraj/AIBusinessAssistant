import { Order } from './order.model';

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
  pendingOrderWorkflow?: OrderWorkflowResponse | null;
}

export interface OrderWorkflowActionRequest {
  action: 'StartPlaceOrder' | 'SelectCustomer' | 'SetOrderItems' | 'EditOrderItems' | 'ConfirmOrder' | 'CancelWorkflow' | 'ShowPendingOrders' | 'SelectPendingOrder' | 'ConfirmCompletion';
  customerId?: number;
  orderId?: number;
  items?: { productId: number; quantity: number }[];
}

export interface OrderWorkflowResponse {
  conversationId: number;
  workflowKind: 'PlaceOrder' | 'PendingOrderCompletion';
  state: 'Idle' | 'SelectingCustomer' | 'SelectingProducts' | 'AwaitingConfirmation' | 'ListingPendingOrders' | 'ReviewingPendingOrders' | 'AwaitingCompletionConfirmation' | 'Completed' | 'Cancelled';
  assistantMessage: string;
  selectedCustomer: { customerId: number; name: string } | null;
  availableCustomers: { customerId: number; name: string }[];
  availableProducts: { productId: number; name: string; sku: string; unitPrice: number; stockQuantity: number }[];
  selectedItems: { productId: number; name: string; quantity: number; unitPrice: number; lineTotal: number; stockAvailable: number; isAvailable: boolean }[];
  orderTotal: number;
  availableActions: string[];
  orderId: number | null;
  orderNumber: string | null;
  pendingOrders: Order[];
  selectedPendingOrder: Order | null;
  completedOrder: Order | null;
}
