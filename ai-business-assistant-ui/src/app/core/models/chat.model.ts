export interface ChatRequest {
  conversationId?: number;
  message: string;
}

export interface ChatResponse {
  conversationId: number;
  userMessage: string;
  assistantMessage: string;
  createdAt: string;
}
