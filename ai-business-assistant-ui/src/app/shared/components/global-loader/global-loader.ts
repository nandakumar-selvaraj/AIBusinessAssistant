import { Component, inject } from '@angular/core';
import { LoadingService } from '../../../core/services/loading.service';

@Component({
  selector: 'app-global-loader',
  templateUrl: './global-loader.html',
  styleUrl: './global-loader.scss',
})
export class GlobalLoader {
  protected readonly loading = inject(LoadingService).loading;
}
