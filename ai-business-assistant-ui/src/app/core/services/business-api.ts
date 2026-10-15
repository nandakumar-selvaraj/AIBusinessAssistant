import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable, map } from 'rxjs';
import { API_BASE_URL } from '../api.config';
import { Customer, CustomerPayload } from '../models/customer.model';
import { Product, ProductPayload } from '../models/product.model';
import { Order } from '../models/order.model';

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
  createOrder(payload: { orderNumber: string; customerId: number; orderItems: { productId: number; quantity: number }[] }): Observable<Order> { return this.http.post<Order>(`${API_BASE_URL}/orders`, payload); }
  deleteOrder(id: number): Observable<void> { return this.http.delete<void>(`${API_BASE_URL}/orders/${id}`); }
}
