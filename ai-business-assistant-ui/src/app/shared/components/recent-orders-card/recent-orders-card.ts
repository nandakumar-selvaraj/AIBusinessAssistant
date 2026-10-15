import { CurrencyPipe, DatePipe } from '@angular/common';
import { Component, input } from '@angular/core';
import { Order } from '../../../core/models/order.model';

@Component({
  imports: [CurrencyPipe, DatePipe],
  selector: 'app-recent-orders-card',
  styleUrl: './recent-orders-card.scss',
  templateUrl: './recent-orders-card.html',
})
export class RecentOrdersCard {
  orders = input.required<Order[]>();
}
