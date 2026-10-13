import { Component } from '@angular/core';
import { StatCard } from '../../shared/components/stat-card/stat-card';
import { AiAssistantCard } from '../../shared/components/ai-assistant-card/ai-assistant-card';
import { RecentOrdersCard } from '../../shared/components/recent-orders-card/recent-orders-card';

@Component({
  imports: [StatCard, AiAssistantCard, RecentOrdersCard],
  selector: 'app-dashboard',
  styleUrl: './dashboard.scss',
  templateUrl: './dashboard.html',
})
export class Dashboard {}
