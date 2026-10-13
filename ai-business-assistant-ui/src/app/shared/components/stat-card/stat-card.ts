import { Component, input } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-stat-card',
  styleUrl: './stat-card.scss',
  templateUrl: './stat-card.html',
})
export class StatCard {
  icon = input.required<string>();      // e.g. 'bi-people'
  value = input.required<string | number>();
  label = input.required<string>();
  secondaryLabel = input.required<string>();
  color = input('text-primary');
}
