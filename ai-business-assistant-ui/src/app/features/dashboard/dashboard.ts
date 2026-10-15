import { CurrencyPipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { BusinessApi } from '../../core/services/business-api';
import { DashboardSummary } from '../../core/models/dashboard.model';
import { StatCard } from '../../shared/components/stat-card/stat-card';
import { RecentOrdersCard } from '../../shared/components/recent-orders-card/recent-orders-card';
import { AiInsights, Insight } from '../../shared/components/ai-insights/ai-insights';

const EMPTY_SUMMARY: DashboardSummary = {
  totalCustomers: 0,
  totalOrders: 0,
  revenue: 0,
  pendingOrders: 0,
  recentOrders: [],
  lowStockProducts: [],
};

@Component({
  imports: [CurrencyPipe, StatCard, RecentOrdersCard, AiInsights],
  selector: 'app-dashboard',
  styleUrl: './dashboard.scss',
  templateUrl: './dashboard.html',
})
export class Dashboard implements OnInit {
  private readonly api = inject(BusinessApi);
  protected readonly summary = signal<DashboardSummary>(EMPTY_SUMMARY);
  protected readonly loading = signal(true);
  protected readonly error = signal('');
  protected readonly insights = signal<Insight[]>([]);

  ngOnInit(): void {
    this.load();
  }

  protected load(): void {
    this.loading.set(true);
    this.error.set('');
    this.api.dashboardSummary().subscribe({
      next: (summary) => {
        this.summary.set(summary);
        this.insights.set(this.createInsights(summary));
        this.loading.set(false);
      },
      error: (response: HttpErrorResponse) => {
        this.error.set(response.error?.detail ?? response.error?.message ?? response.error?.title ?? 'Dashboard data could not be loaded. Check the API connection and try again.');
        this.loading.set(false);
      },
    });
  }

  private createInsights(summary: DashboardSummary): Insight[] {
    return [
      {
        icon: 'bi-clock',
        color: summary.pendingOrders ? 'text-warning' : 'text-success',
        text: summary.pendingOrders
          ? `${summary.pendingOrders} ${summary.pendingOrders === 1 ? 'order is' : 'orders are'} awaiting processing.`
          : 'There are no pending orders.',
        link: '/items',
      },
      {
        icon: 'bi-box-seam',
        color: summary.lowStockProducts.length ? 'text-danger' : 'text-success',
        text: summary.lowStockProducts.length
          ? `${summary.lowStockProducts.length} active ${summary.lowStockProducts.length === 1 ? 'product has' : 'products have'} 5 or fewer units in stock.`
          : 'No active products have 5 or fewer units in stock.',
        link: '/products',
      },
    ];
  }
}
