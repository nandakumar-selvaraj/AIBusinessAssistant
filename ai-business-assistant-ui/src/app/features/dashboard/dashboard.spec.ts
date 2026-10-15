import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { TestBed } from '@angular/core/testing';
import { Dashboard } from './dashboard';
import { DashboardSummary } from '../../core/models/dashboard.model';

describe('Dashboard', () => {
  let http: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Dashboard],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();
    http = TestBed.inject(HttpTestingController);
  });

  afterEach(() => http.verify());

  it('renders zero metrics and empty states when the API has no records', () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();

    http.expectOne('/api/dashboard/summary').flush(emptySummary);
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    expect(page.textContent).toContain('Total Customers');
    expect(page.textContent).toContain('No orders have been recorded yet.');
    expect(page.textContent).toContain('No low-stock products.');
    expect(page.querySelector('.alert-danger')).toBeNull();
  });

  it('renders metric totals, recent orders, and low-stock products from the API', () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();

    http.expectOne('/api/dashboard/summary').flush({
      ...emptySummary,
      totalCustomers: 2,
      totalOrders: 3,
      revenue: 1250.5,
      pendingOrders: 1,
      recentOrders: [{ orderId: 4, orderNumber: 'ORD-004', customerId: 2, customerName: 'Northwind', orderDate: '2026-10-15T10:00:00Z', status: 'Pending', totalAmount: 250, orderItems: [] }],
      lowStockProducts: [{ productId: 8, name: 'Widget', sku: 'W-008', description: null, unitPrice: 10, stockQuantity: 2, isActive: true, createdAt: '2026-10-01T00:00:00Z' }],
    } satisfies DashboardSummary);
    fixture.detectChanges();

    const page = fixture.nativeElement as HTMLElement;
    expect(page.textContent).toContain('2');
    expect(page.textContent).toContain('3');
    expect(page.textContent).toContain('₹1,250.5');
    expect(page.textContent).toContain('ORD-004');
    expect(page.textContent).toContain('Northwind');
    expect(page.textContent).toContain('Widget');
    expect(page.textContent).toContain('1 order is awaiting processing.');
  });

  it('shows an API error and allows retrying the request', () => {
    const fixture = TestBed.createComponent(Dashboard);
    fixture.detectChanges();
    http.expectOne('/api/dashboard/summary').flush(
      { title: 'Dashboard service unavailable' },
      { status: 500, statusText: 'Internal Server Error' },
    );
    fixture.detectChanges();

    expect((fixture.nativeElement as HTMLElement).textContent).toContain('Dashboard service unavailable');
    const retry = (fixture.nativeElement as HTMLElement).querySelector('.alert-danger button') as HTMLButtonElement;
    retry.click();
    fixture.detectChanges();
    http.expectOne('/api/dashboard/summary').flush(emptySummary);
  });
});

const emptySummary: DashboardSummary = {
  totalCustomers: 0,
  totalOrders: 0,
  revenue: 0,
  pendingOrders: 0,
  recentOrders: [],
  lowStockProducts: [],
};
