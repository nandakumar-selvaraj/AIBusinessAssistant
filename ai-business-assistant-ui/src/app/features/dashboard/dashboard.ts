import { Component } from '@angular/core';
import { StatCard } from '../../shared/components/stat-card/stat-card';
import { RecentOrdersCard } from '../../shared/components/recent-orders-card/recent-orders-card';
import { AiInsights, Insight } from '../../shared/components/ai-insights/ai-insights';

@Component({
  imports: [StatCard, RecentOrdersCard, AiInsights],
  selector: 'app-dashboard',
  styleUrl: './dashboard.scss',
  templateUrl: './dashboard.html',
})
export class Dashboard {
  insights: Insight[] = [
    { icon: 'bi-clock',             color: 'text-warning', text: '5 orders are awaiting processing.',     link: '/orders' },
    { icon: 'bi-box-seam',          color: 'text-danger',  text: '3 products are running low on stock.',  link: '/products' },
    { icon: 'bi-graph-up-arrow',    color: 'text-success', text: 'Revenue has increased this month.' }
  ];
}
