import { Injectable, signal } from '@angular/core';

@Injectable({ providedIn: 'root' })
export class LoadingService {
  private pendingRequests = 0;
  readonly loading = signal(false);

  beginRequest(): void {
    this.pendingRequests += 1;
    this.loading.set(true);
  }

  endRequest(): void {
    this.pendingRequests = Math.max(0, this.pendingRequests - 1);
    this.loading.set(this.pendingRequests > 0);
  }
}
