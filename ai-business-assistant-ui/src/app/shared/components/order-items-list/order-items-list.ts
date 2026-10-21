import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { Order } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-items-list',
  imports: [CurrencyPipe, DatePipe],
  templateUrl: './order-items-list.html',
  styleUrl: './order-items-list.scss',
})
export class OrderItemsList {
  @Input({ required: true }) orders: Order[] = [];
  @Input() showActions = false;
  @Input() completingId: number | null = null;
  @Output() completeOrder = new EventEmitter<Order>();
  @Output() deleteOrder = new EventEmitter<Order>();

  protected readonly expanded = signal<number | null>(null);

  protected get ordersForDisplay(): Order[] {
    return [...this.orders].sort((left, right) =>
      Number(right.status.toLowerCase() === 'pending') - Number(left.status.toLowerCase() === 'pending'));
  }

  protected toggle(orderId: number): void {
    this.expanded.set(this.expanded() === orderId ? null : orderId);
  }
}
