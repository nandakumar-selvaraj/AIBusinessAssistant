import { CommonModule } from "@angular/common";
import { Component, Input } from "@angular/core";
import { RouterLink } from "@angular/router";

export interface Insight {
  icon: string;    // e.g. 'bi-clock'
  color: string;   // Bootstrap text color class, e.g. 'text-warning'
  text: string;
  link?: string;   // optional route, e.g. '/orders'
}

@Component({
  imports: [RouterLink, CommonModule],
  selector: "app-ai-insights",
  styleUrl: "./ai-insights.scss",
  templateUrl: "./ai-insights.html",
})
export class AiInsights {
  @Input({ required: true }) insights: Insight[] = [];
  @Input() assistantLink = '/ai-assistant';
}
