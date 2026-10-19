import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { Customer, CustomerPayload } from '../models/customer.model';
import { Product, ProductPayload } from '../models/product.model';
import { Order } from '../models/order.model';
import { DashboardSummary } from '../models/dashboard.model';
import { ChatRequest, ChatResponse, OrderWorkflowActionRequest, OrderWorkflowResponse } from '../models/chat.model';

@Injectable({ providedIn: 'root' })
export class BusinessApi {
  private readonly http = inject(HttpClient);

  customers(): Observable<Customer[]> { return this.http.get<Customer[]>(`${API_BASE_URL}/customers`); }
  createCustomer(payload: CustomerPayload): Observable<void> { return this.http.post<Customer>(`${API_BASE_URL}/customers`, payload).pipe(map(() => undefined)); }
  updateCustomer(id: number, payload: CustomerPayload): Observable<void> { return this.http.put<void>(`${API_BASE_URL}/customers/${id}`, payload); }
  deleteCustomer(id: number): Observable<void> { return this.http.delete<void>(`${API_BASE_URL}/customers/${id}`); }

  products(): Observable<Product[]> { return this.http.get<Product[]>(`${API_BASE_URL}/products`); }
  createProduct(payload: ProductPayload): Observable<void> { return this.http.post<Product>(`${API_BASE_URL}/products`, payload).pipe(map(() => undefined)); }
  updateProduct(id: number, payload: ProductPayload): Observable<void> { return this.http.put<void>(`${API_BASE_URL}/products/${id}`, payload); }
  deleteProduct(id: number): Observable<void> { return this.http.delete<void>(`${API_BASE_URL}/products/${id}`); }

  orders(): Observable<Order[]> { return this.http.get<Order[]>(`${API_BASE_URL}/orders`); }
  completeOrder(id: number): Observable<Order> { return this.http.post<Order>(`${API_BASE_URL}/orders/${id}/complete`, {}); }
  dashboardSummary(): Observable<DashboardSummary> { return this.http.get<DashboardSummary>(`${API_BASE_URL}/dashboard/summary`); }
  chat(request: ChatRequest): Observable<ChatResponse> { return this.http.post<ChatResponse>(`${API_BASE_URL}/chat`, request); }
  orderWorkflowAction(conversationId: number, request: OrderWorkflowActionRequest): Observable<OrderWorkflowResponse> {
    return this.http.post<OrderWorkflowResponse>(`${API_BASE_URL}/chat/${conversationId}/order-workflow`, request);
  }
  activeOrderWorkflow(conversationId: number): Observable<OrderWorkflowResponse | null> {
    return this.http.get<OrderWorkflowResponse | null>(`${API_BASE_URL}/chat/${conversationId}/order-workflow`);
  }
  createOrder(payload: { orderNumber: string; customerId: number; orderItems: { productId: number; quantity: number }[] }): Observable<Order> { return this.http.post<Order>(`${API_BASE_URL}/orders`, payload); }
  deleteOrder(id: number): Observable<void> { return this.http.delete<void>(`${API_BASE_URL}/orders/${id}`); }
}
