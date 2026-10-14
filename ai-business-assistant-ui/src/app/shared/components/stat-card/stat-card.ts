import { Component, input } from '@angular/core';

@Component({
  imports: [],
  selector: 'app-stat-card',
  styleUrl: './stat-card.scss',
  templateUrl: './stat-card.html',
})
export class StatCard {
  icon = input.required<string>();          // e.g. 'bi-bag'
  value = input.required<string | number>();
  label = input.required<string>();
  secondaryLabel = input.required<string>();
  percentage = input<number | string | null>(null); // e.g. 4.9
  color = input('text-primary');
}
