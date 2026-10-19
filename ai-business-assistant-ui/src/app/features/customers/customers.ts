import { Component, OnInit, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpErrorResponse } from '@angular/common/http';
import { Customer, CustomerPayload } from '../../core/models/customer.model';
import { BusinessApi } from '../../core/services/business-api';
import { ConfirmModal } from '../../shared/components/confirm-modal/confirm-modal';
import { CustomerList } from '../../shared/components/customer-list/customer-list';

@Component({ selector: 'app-customers', imports: [FormsModule, ConfirmModal, CustomerList], templateUrl: './customers.html', styleUrl: './customers.scss' })
export class Customers implements OnInit {
  private readonly api = inject(BusinessApi);
  protected readonly rows = signal<Customer[]>([]);
  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal('');
  protected readonly editingId = signal<number | null>(null);
  protected readonly query = signal('');
  protected readonly pendingDelete = signal<Customer | null>(null);
  protected form: CustomerPayload = this.emptyForm();

  ngOnInit(): void { this.load(); }
  protected get filtered(): Customer[] {
    const q = this.query().trim().toLowerCase();
    return q ? this.rows().filter((x) => [x.name, x.email, x.phone].some((v) => v?.toLowerCase().includes(q))) : this.rows();
  }
  protected startCreate(): void { this.editingId.set(null); this.form = this.emptyForm(); this.error.set(''); }
  protected edit(row: Customer): void {
    this.editingId.set(row.customerId);
    this.form = { name: row.name, email: row.email, phone: row.phone, address: row.address };
    this.error.set('');
  }
  protected save(): void {
    this.error.set(''); this.saving.set(true);
    const payload = { ...this.form, name: this.form.name.trim(), email: this.clean(this.form.email), phone: this.clean(this.form.phone), address: this.clean(this.form.address) };
    const request = this.editingId() === null ? this.api.createCustomer(payload) : this.api.updateCustomer(this.editingId()!, payload);
    request.subscribe({ next: () => { this.saving.set(false); this.startCreate(); this.load(); }, error: (e: HttpErrorResponse) => { this.saving.set(false); this.error.set(this.message(e)); } });
  }
  protected confirmDelete(): void {
    const row = this.pendingDelete();
    if (!row) return;
    this.pendingDelete.set(null);
    this.error.set(''); this.api.deleteCustomer(row.customerId).subscribe({ next: () => this.load(), error: (e: HttpErrorResponse) => this.error.set(this.message(e)) });
  }
  private load(): void {
    this.loading.set(true);
    this.api.customers().subscribe({ next: (data) => { this.rows.set(data); this.loading.set(false); }, error: (e: HttpErrorResponse) => { this.error.set(this.message(e)); this.loading.set(false); } });
  }
  private emptyForm(): CustomerPayload { return { name: '', email: null, phone: null, address: null }; }
  private clean(value: string | null): string | null { return value?.trim() || null; }
  private message(e: HttpErrorResponse): string { return e.error?.message ?? 'The request failed. Check that the API is running and try again.'; }
}
