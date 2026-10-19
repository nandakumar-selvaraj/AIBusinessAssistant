import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BusinessApi } from '../../core/services/business-api';
import { ChatRequest } from '../../core/models/chat.model';

type ChatRole = 'user' | 'assistant';
type ResponseView = 'grid' | 'json';
interface ChatEntry {
  role: ChatRole;
  content: string;
  createdAt: Date;
  jsonText?: string;
  gridColumns?: string[];
  gridRows?: string[][];
  isList?: boolean;
  viewMode?: ResponseView;
}

const MAX_MESSAGE_LENGTH = 4000;

@Component({
  selector: 'app-ai-assistant',
  imports: [FormsModule, DatePipe],
  templateUrl: './ai-assistant.html',
  styleUrl: './ai-assistant.scss',
})
export class AiAssistant {
  private readonly api = inject(BusinessApi);

  protected readonly messages = signal<ChatEntry[]>([]);
  protected readonly conversationId = signal<number | null>(null);
  protected readonly sending = signal(false);
  protected readonly error = signal('');
  protected readonly maxMessageLength = MAX_MESSAGE_LENGTH;
  protected draft = '';

  protected readonly suggestions = [
    'Last month registered users',
    'Highest order of the month',
    'Top 5 orders of the month',
    'List pending orders',
    'Active products with 5 or fewer units in stock',
  ];

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
        this.messages.update((entries) => [...entries, this.toAssistantEntry(response.assistantMessage, new Date(response.createdAt))]);
        this.sending.set(false);
      },
      error: (response: HttpErrorResponse) => {
        this.error.set(this.errorMessage(response));
        this.sending.set(false);
      },
    });
  }

  protected toggleResponseView(index: number): void {
    this.messages.update((entries) => entries.map((entry, entryIndex) =>
      entryIndex === index && entry.role === 'assistant'
        ? { ...entry, viewMode: entry.viewMode === 'json' ? 'grid' : 'json' }
        : entry));
  }

  protected startNewConversation(): void {
    if (this.sending()) return;
    this.messages.set([]);
    this.conversationId.set(null);
    this.error.set('');
    this.draft = '';
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

  private toAssistantEntry(content: string, createdAt: Date): ChatEntry {
    let data: unknown;
    try {
      data = JSON.parse(content);
    } catch {
      data = { response: content };
    }

    if (Array.isArray(data)) {
      const recordsOnly = data.every((item) => item !== null && typeof item === 'object' && !Array.isArray(item));
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
      return { role: 'assistant', content, createdAt, jsonText: JSON.stringify(data, null, 2), gridColumns, gridRows, isList: true, viewMode: 'grid' };
    }

    const record = data !== null && typeof data === 'object' ? data as Record<string, unknown> : { response: data };
    return {
      role: 'assistant',
      content,
      createdAt,
      jsonText: JSON.stringify(data, null, 2),
      gridColumns: ['Field', 'Value'],
      gridRows: Object.entries(record).map(([key, value]) => [key, this.cellValue(value)]),
      isList: false,
      viewMode: 'grid',
    };
  }

  private cellValue(value: unknown): string {
    if (value === null || value === undefined) return '—';
    if (typeof value === 'object') return JSON.stringify(value);
    return String(value);
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
