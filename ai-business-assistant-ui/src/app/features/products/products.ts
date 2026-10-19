import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { ConfirmModal } from '../../shared/components/confirm-modal/confirm-modal';
import { ProductList } from '../../shared/components/product-list/product-list';
import { Product, ProductPayload } from '../../core/models/product.model';
import { BusinessApi } from '../../core/services/business-api';

@Component({ selector: 'app-products', imports: [FormsModule, ConfirmModal, ProductList], templateUrl: './products.html', styleUrl: './products.scss' })
export class Products implements OnInit {
  private readonly api = inject(BusinessApi);
  protected readonly rows = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected readonly editingId = signal<number | null>(null);
  protected readonly query = signal('');
  protected readonly pendingDelete = signal<Product | null>(null);
  protected form: ProductPayload = this.emptyForm();
  ngOnInit(): void { this.load(); }
  protected get filtered(): Product[] { const q = this.query().trim().toLowerCase(); return q ? this.rows().filter((x) => [x.name, x.sku, x.description].some((v) => v?.toLowerCase().includes(q))) : this.rows(); }
  protected startCreate(): void { this.editingId.set(null); this.form = this.emptyForm(); this.error.set(''); }
  protected edit(row: Product): void { this.editingId.set(row.productId); this.form = { name: row.name, sku: row.sku, description: row.description, unitPrice: row.unitPrice, stockQuantity: row.stockQuantity, isActive: row.isActive }; this.error.set(''); }
  protected save(): void {
    this.error.set(''); this.saving.set(true);
    const payload = { ...this.form, name: this.form.name.trim(), sku: this.form.sku.trim(), description: this.form.description?.trim() || null };
    const request = this.editingId() === null ? this.api.createProduct(payload) : this.api.updateProduct(this.editingId()!, payload);
    request.subscribe({ next: () => { this.saving.set(false); this.startCreate(); this.load(); }, error: (e: HttpErrorResponse) => { this.saving.set(false); this.error.set(this.message(e)); } });
  }
  protected confirmDelete(): void { const row = this.pendingDelete(); if (!row) return; this.pendingDelete.set(null); this.error.set(''); this.api.deleteProduct(row.productId).subscribe({ next: () => this.load(), error: (e: HttpErrorResponse) => this.error.set(this.message(e)) }); }
  private load(): void { this.loading.set(true); this.api.products().subscribe({ next: (data) => { this.rows.set(data); this.loading.set(false); }, error: (e: HttpErrorResponse) => { this.error.set(this.message(e)); this.loading.set(false); } }); }
  private emptyForm(): ProductPayload { return { name: '', sku: '', description: null, unitPrice: 0, stockQuantity: 0, isActive: true }; }
  private message(e: HttpErrorResponse): string { return e.error?.message ?? 'The request failed. Check that the API is running and try again.'; }
}
