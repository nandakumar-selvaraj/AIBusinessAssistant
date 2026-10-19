import { CurrencyPipe, DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BusinessApi } from '../../core/services/business-api';
import { ChatRequest, OrderWorkflowActionRequest, OrderWorkflowResponse } from '../../core/models/chat.model';
import { Order } from '../../core/models/order.model';
import { Customer } from '../../core/models/customer.model';
import { Product } from '../../core/models/product.model';
import { OrderItemsList } from '../../shared/components/order-items-list/order-items-list';
import { CustomerList } from '../../shared/components/customer-list/customer-list';
import { ProductList } from '../../shared/components/product-list/product-list';

type ChatRole = 'user' | 'assistant';
interface ChatEntry {
  role: ChatRole;
  content: string;
  createdAt: Date;
  gridColumns?: string[];
  gridRows?: string[][];
  isList?: boolean;
  orders?: Order[];
  customers?: Customer[];
  products?: Product[];
  orderWorkflow?: OrderWorkflowResponse;
}

const MAX_MESSAGE_LENGTH = 4000;

@Component({
  selector: 'app-ai-assistant',
  imports: [FormsModule, DatePipe, CurrencyPipe, OrderItemsList, CustomerList, ProductList],
  templateUrl: './ai-assistant.html',
  styleUrl: './ai-assistant.scss',
})
export class AiAssistant implements OnInit {
  private readonly api = inject(BusinessApi);

  protected readonly messages = signal<ChatEntry[]>([]);
  protected readonly conversationId = signal<number | null>(null);
  protected readonly sending = signal(false);
  protected readonly error = signal('');
  protected readonly maxMessageLength = MAX_MESSAGE_LENGTH;
  protected draft = '';
  protected orderQuantities: Record<number, number> = {};

  protected readonly suggestions = [
    'Show me all customers',
    'Last 5 orders',
    'Last month registered users',
    'Highest order of the month',
    'Top 5 orders of the month',
    'List pending orders',
    'Active products with 5 or fewer units in stock',
    'Place an order',
  ];

  ngOnInit(): void {
    const savedConversationId = Number(sessionStorage.getItem('bai-active-conversation'));
    if (!Number.isInteger(savedConversationId) || savedConversationId <= 0) return;
    this.conversationId.set(savedConversationId);
    this.api.activeOrderWorkflow(savedConversationId).subscribe({
      next: (workflow) => {
        if (!workflow) return;
        this.loadOrderQuantities(workflow);
        this.messages.set([{ role: 'assistant', content: workflow.assistantMessage, createdAt: new Date(), orderWorkflow: workflow }]);
      },
      error: () => sessionStorage.removeItem('bai-active-conversation'),
    });
  }

  protected send(): void {
    const message = this.draft.trim();
    if (!message || this.sending()) return;
    if (message.length > MAX_MESSAGE_LENGTH) {
      this.error.set(`Messages can be up to ${MAX_MESSAGE_LENGTH} characters.`);
      return;
    }

    this.error.set('');
    this.messages.update((entries) => [...entries, { role: 'user', content: message, createdAt: new Date() }]);
    this.draft = '';
    this.sending.set(true);

    const request: ChatRequest = { message };
    const currentConversationId = this.conversationId();
    if (currentConversationId !== null) request.conversationId = currentConversationId;

    this.api.chat(request).subscribe({
      next: (response) => {
        this.conversationId.set(response.conversationId);
        sessionStorage.setItem('bai-active-conversation', String(response.conversationId));
        this.messages.update((entries) => [...entries, this.toAssistantEntry(response.assistantMessage, new Date(response.createdAt), message, response.orderWorkflow ?? undefined)]);
        if (response.orderWorkflow) this.loadOrderQuantities(response.orderWorkflow);
        this.sending.set(false);
      },
      error: (response: HttpErrorResponse) => {
        this.error.set(this.errorMessage(response));
        this.sending.set(false);
      },
    });
  }

  protected startNewConversation(): void {
    if (this.sending()) return;
    this.messages.set([]);
    this.conversationId.set(null);
    sessionStorage.removeItem('bai-active-conversation');
    this.error.set('');
    this.draft = '';
    this.orderQuantities = {};
  }

  protected useSuggestion(suggestion: string): void {
    if (!this.sending()) this.draft = suggestion;
  }

  protected onEnter(event: KeyboardEvent): void {
    if (event.key === 'Enter' && !event.shiftKey) {
      event.preventDefault();
      this.send();
    }
  }

  protected selectOrderCustomer(customerId: number): void {
    this.runOrderAction({ action: 'SelectCustomer', customerId });
  }

  protected setOrderQuantity(productId: number, quantity: number | string): void {
    const parsed = Number(quantity);
    this.orderQuantities[productId] = Number.isInteger(parsed) && parsed > 0 ? parsed : 0;
  }

  protected reviewOrder(): void {
    const items = Object.entries(this.orderQuantities)
      .filter(([, quantity]) => Number.isInteger(quantity) && quantity > 0)
      .map(([productId, quantity]) => ({ productId: Number(productId), quantity }));
    this.runOrderAction({ action: 'SetOrderItems', items });
  }

  protected confirmOrder(): void { this.runOrderAction({ action: 'ConfirmOrder' }); }
  protected editOrderItems(): void { this.runOrderAction({ action: 'EditOrderItems' }); }
  protected canConfirmOrder(workflow: OrderWorkflowResponse): boolean {
    return workflow.selectedItems.length > 0 && workflow.selectedItems.every((item) =>
      item.isAvailable && item.stockAvailable >= item.quantity);
  }
  protected cancelOrder(): void { this.runOrderAction({ action: 'CancelWorkflow' }); }
  protected startAnotherOrder(): void { this.runOrderAction({ action: 'StartPlaceOrder' }); }

  private runOrderAction(action: OrderWorkflowActionRequest): void {
    const conversationId = this.conversationId();
    if (conversationId === null || this.sending()) return;
    this.error.set('');
    this.sending.set(true);
    this.api.orderWorkflowAction(conversationId, action).subscribe({
      next: (workflow) => {
        this.messages.update((entries) => [...entries, {
          role: 'assistant', content: workflow.assistantMessage, createdAt: new Date(), orderWorkflow: workflow,
        }]);
        this.loadOrderQuantities(workflow);
        this.sending.set(false);
      },
      error: (response: HttpErrorResponse) => {
        this.error.set(this.errorMessage(response));
        this.sending.set(false);
      },
    });
  }

  private loadOrderQuantities(workflow: OrderWorkflowResponse): void {
    this.orderQuantities = Object.fromEntries(workflow.selectedItems.map((item) => [item.productId, item.quantity]));
  }

  private toAssistantEntry(content: string, createdAt: Date, prompt: string, orderWorkflow?: OrderWorkflowResponse): ChatEntry {
    if (orderWorkflow) return { role: 'assistant', content, createdAt, orderWorkflow };
    const data = this.parseAssistantContent(content);
    const isCustomerQuery = /\bcustomers?\b/i.test(prompt) && !/\bregistered\s+customers?\b/i.test(prompt);
    const isProductQuery = /\bproducts?\b/i.test(prompt);

    if (Array.isArray(data)) {
      const recordsOnly = data.every((item) => item !== null && typeof item === 'object' && !Array.isArray(item));
      if (isProductQuery && (data.length === 0 || (recordsOnly && this.isProductRecord(data[0] as Record<string, unknown>)))) {
        return {
          role: 'assistant',
          content,
          createdAt,
          products: data.map((item) => this.toProduct(item as Record<string, unknown>)),
        };
      }
      if (isCustomerQuery && (data.length === 0 || (recordsOnly && this.isCustomerRecord(data[0] as Record<string, unknown>)))) {
        return {
          role: 'assistant',
          content,
          createdAt,
          customers: data.map((item) => this.toCustomer(item as Record<string, unknown>)),
        };
      }
      if (recordsOnly && data.length > 0 && this.isOrderRecord(data[0] as Record<string, unknown>)) {
        return {
          role: 'assistant',
          content,
          createdAt,
          orders: data.map((item) => this.toOrder(item as Record<string, unknown>)),
        };
      }
      const columns = recordsOnly
        ? [...new Set(data.flatMap((item) => Object.keys(item as Record<string, unknown>)))]
        : [];
      const gridColumns = recordsOnly && columns.length ? columns : ['#', 'Value'];
      const gridRows = data.map((item, rowIndex) => {
        if (recordsOnly) {
          const record = item as Record<string, unknown>;
          return gridColumns.map((column) => this.cellValue(record[column]));
        }
        return [String(rowIndex + 1), this.cellValue(item)];
      });
      return { role: 'assistant', content, createdAt, gridColumns, gridRows, isList: true };
    }

    const record = data !== null && typeof data === 'object' ? data as Record<string, unknown> : { response: data };
    if (isCustomerQuery && this.isCustomerRecord(record)) {
      return {
        role: 'assistant',
        content,
        createdAt,
        customers: [this.toCustomer(record)],
      };
    }
    if (isProductQuery && this.isProductRecord(record)) {
      return {
        role: 'assistant',
        content,
        createdAt,
        products: [this.toProduct(record)],
      };
    }
    if (this.isOrderRecord(record)) {
      return {
        role: 'assistant',
        content,
        createdAt,
        orders: [this.toOrder(record)],
      };
    }

    return {
      role: 'assistant',
      content,
      createdAt,
      gridColumns: ['Field', 'Value'],
      gridRows: Object.entries(record).map(([key, value]) => [key, this.cellValue(value)]),
      isList: false,
    };
  }

  private isProductRecord(record: Record<string, unknown>): boolean {
    const keys = new Set(Object.keys(record).map((key) => key.toLowerCase()));
    return keys.has('productid') && keys.has('name') && keys.has('sku');
  }

  private toProduct(record: Record<string, unknown>): Product {
    return {
      productId: Number(this.getProperty(record, 'productId')) || 0,
      name: String(this.getProperty(record, 'name') ?? 'Product'),
      sku: String(this.getProperty(record, 'sku') ?? ''),
      description: this.getProperty(record, 'description') ? String(this.getProperty(record, 'description')) : null,
      unitPrice: Number(this.getProperty(record, 'unitPrice')) || 0,
      stockQuantity: Number(this.getProperty(record, 'stockQuantity')) || 0,
      isActive: Boolean(this.getProperty(record, 'isActive')),
      createdAt: String(this.getProperty(record, 'createdAt') ?? ''),
    };
  }

  private isCustomerRecord(record: Record<string, unknown>): boolean {
    const keys = new Set(Object.keys(record).map((key) => key.toLowerCase()));
    return keys.has('customerid') && keys.has('name');
  }

  private toCustomer(record: Record<string, unknown>): Customer {
    return {
      customerId: Number(this.getProperty(record, 'customerId')) || 0,
      name: String(this.getProperty(record, 'name') ?? 'Customer'),
      email: this.getProperty(record, 'email') ? String(this.getProperty(record, 'email')) : null,
      phone: null,
      address: null,
      createdAt: String(this.getProperty(record, 'createdAt') ?? ''),
    };
  }

  private isOrderRecord(record: Record<string, unknown>): boolean {
    const keys = new Set(Object.keys(record).map((key) => key.toLowerCase()));
    return keys.has('ordernumber') && keys.has('totalamount') && (keys.has('items') || keys.has('orderitems'));
  }

  private toOrder(record: Record<string, unknown>): Order {
    const sourceItems = this.getProperty(record, 'items', 'orderItems');
    const items = Array.isArray(sourceItems) ? sourceItems.map((source) => {
      const item = source !== null && typeof source === 'object' ? source as Record<string, unknown> : {};
      const quantity = Number(this.getProperty(item, 'quantity')) || 0;
      const unitPrice = Number(this.getProperty(item, 'unitPrice')) || 0;
      return {
        productId: Number(this.getProperty(item, 'productId')) || 0,
        productName: String(this.getProperty(item, 'productName', 'name') ?? 'Item'),
        quantity,
        unitPrice,
        lineTotal: Number(this.getProperty(item, 'lineTotal')) || quantity * unitPrice,
      };
    }) : [];

    return {
      orderId: Number(this.getProperty(record, 'orderId')) || 0,
      orderNumber: String(this.getProperty(record, 'orderNumber') ?? 'Order'),
      customerId: Number(this.getProperty(record, 'customerId')) || 0,
      customerName: String(this.getProperty(record, 'customerName') ?? 'Customer'),
      orderDate: String(this.getProperty(record, 'orderDate') ?? ''),
      status: String(this.getProperty(record, 'status') ?? 'Unknown'),
      totalAmount: Number(this.getProperty(record, 'totalAmount')) || 0,
      orderItems: items,
    };
  }

  private getProperty(record: Record<string, unknown>, ...names: string[]): unknown {
    const key = Object.keys(record).find((candidate) => names.some((name) => candidate.toLowerCase() === name.toLowerCase()));
    return key ? record[key] : undefined;
  }

  private cellValue(value: unknown): string {
    if (value === null || value === undefined) return '—';
    if (typeof value === 'object') return JSON.stringify(value);
    return String(value);
  }

  private parseAssistantContent(content: string): unknown {
    let candidate = content.trim();
    const fencedJson = candidate.match(/^```(?:json)?\s*([\s\S]*?)\s*```$/i);
    if (fencedJson) candidate = fencedJson[1].trim();

    try {
      const parsed: unknown = JSON.parse(candidate);
      if (typeof parsed === 'string') {
        const nested = parsed.trim();
        if (nested.startsWith('[') || nested.startsWith('{')) {
          try {
            return JSON.parse(nested) as unknown;
          } catch {
            return { response: parsed };
          }
        }
      }
      return parsed;
    } catch {
      // Some local models add a short preamble around otherwise valid JSON.
      const start = candidate.search(/[\[{]/);
      const end = Math.max(candidate.lastIndexOf(']'), candidate.lastIndexOf('}'));
      if (start >= 0 && end > start) {
        try {
          return JSON.parse(candidate.slice(start, end + 1)) as unknown;
        } catch {
          // Fall through and display the message as a single response field.
        }
      }
      return { response: content };
    }
  }

  private errorMessage(response: HttpErrorResponse): string {
    const body = response.error;
    const validationMessages = body?.errors ? Object.values(body.errors).flat() : [];
    return validationMessages[0] as string
      ?? body?.detail
      ?? body?.title
      ?? body?.message
      ?? 'The assistant could not respond. Check the API connection and try again.';
  }
}
