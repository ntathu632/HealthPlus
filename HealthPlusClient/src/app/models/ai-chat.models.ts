export interface AiMessage {
  id: string;
  role: 'user' | 'assistant';
  content: string;
  createdAt: string;
}

export interface AiChatResponse {
  conversationId: string;
  reply: AiMessage;
}

export interface AiConversationSummary {
  id: string;
  title?: string;
  createdAt: string;
}

export interface SendAiMessageRequest {
  conversationId?: string;
  message: string;
}
