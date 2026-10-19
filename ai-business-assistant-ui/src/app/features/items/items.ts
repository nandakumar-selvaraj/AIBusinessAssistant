import { CurrencyPipe } from '@angular/common';
import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { BusinessApi } from '../../core/services/business-api';
import { Customer } from '../../core/models/customer.model';
import { Product } from '../../core/models/product.model';
import { Order } from '../../core/models/order.model';
import { ConfirmModal } from '../../shared/components/confirm-modal/confirm-modal';
import { OrderItemsList } from '../../shared/components/order-items-list/order-items-list';

interface DraftItem { productId: number | null; quantity: number; }

@Component({ selector: 'app-items', imports: [FormsModule, CurrencyPipe, ConfirmModal, OrderItemsList], templateUrl: './items.html', styleUrl: './items.scss' })
export class Items implements OnInit {
  private readonly api = inject(BusinessApi);
  protected readonly orders = signal<Order[]>([]);
  protected readonly customers = signal<Customer[]>([]);
  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected readonly pendingDelete = signal<Order | null>(null);
  protected readonly pendingComplete = signal<Order | null>(null);
  protected readonly completingId = signal<number | null>(null);
  protected customerId: number | null = null;
  protected orderNumber = '';
  protected draftItems: DraftItem[] = [{ productId: null, quantity: 1 }];

  ngOnInit(): void {
    this.api.customers().subscribe({ next: (x) => this.customers.set(x), error: (e: HttpErrorResponse) => this.error.set(this.message(e)) });
    this.api.products().subscribe({ next: (x) => this.products.set(x.filter((p) => p.isActive)), error: (e: HttpErrorResponse) => this.error.set(this.message(e)) });
    this.load();
  }
  protected addItem(): void { this.draftItems = [...this.draftItems, { productId: null, quantity: 1 }]; }
  protected removeItem(index: number): void { this.draftItems = this.draftItems.filter((_, i) => i !== index); if (!this.draftItems.length) this.addItem(); }
  protected product(id: number | null): Product | undefined { return this.products().find((p) => p.productId === Number(id)); }
  protected lineTotal(item: DraftItem): number { return (this.product(item.productId)?.unitPrice ?? 0) * (Number(item.quantity) || 0); }
  protected get draftTotal(): number { return this.draftItems.reduce((sum, item) => sum + this.lineTotal(item), 0); }
  protected save(): void {
    this.error.set('');
    if (!this.customerId || !this.orderNumber.trim() || this.draftItems.some((i) => !i.productId || i.quantity < 1)) { this.error.set('Enter an order number, choose a customer, and add at least one valid item.'); return; }
    this.saving.set(true);
    this.api.createOrder({ orderNumber: this.orderNumber.trim(), customerId: this.customerId, orderItems: this.draftItems.map((i) => ({ productId: Number(i.productId), quantity: Number(i.quantity) })) }).subscribe({
      next: () => { this.saving.set(false); this.orderNumber = ''; this.customerId = null; this.draftItems = [{ productId: null, quantity: 1 }]; this.load(); },
      error: (e: HttpErrorResponse) => { this.saving.set(false); this.error.set(this.message(e)); }
    });
  }
  protected confirmDelete(): void {
    const order = this.pendingDelete();
    if (!order) return;
    this.pendingDelete.set(null);
    this.api.deleteOrder(order.orderId).subscribe({ next: () => this.load(), error: (e: HttpErrorResponse) => this.error.set(this.message(e)) });
  }
  protected confirmComplete(): void {
    const order = this.pendingComplete();
    if (!order) return;
    this.error.set('');
    this.completingId.set(order.orderId);
    this.api.completeOrder(order.orderId).subscribe({
      next: () => {
        this.pendingComplete.set(null);
        this.completingId.set(null);
        this.load();
      },
      error: (e: HttpErrorResponse) => {
        this.pendingComplete.set(null);
        this.completingId.set(null);
        this.error.set(this.message(e));
      },
    });
  }
  private load(): void { this.loading.set(true); this.api.orders().subscribe({ next: (x) => { this.orders.set(x); this.loading.set(false); }, error: (e: HttpErrorResponse) => { this.error.set(this.message(e)); this.loading.set(false); } }); }
  private message(e: HttpErrorResponse): string { return e.error?.message ?? 'The request failed. Check that the API is running and try again.'; }
}
