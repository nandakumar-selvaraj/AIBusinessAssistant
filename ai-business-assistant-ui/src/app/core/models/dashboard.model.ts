import { Order } from './order.model';
import { Product } from './product.model';

export interface DashboardSummary {
  totalCustomers: number;
  totalOrders: number;
  revenue: number;
  pendingOrders: number;
  recentOrders: Order[];
  lowStockProducts: Product[];
}
