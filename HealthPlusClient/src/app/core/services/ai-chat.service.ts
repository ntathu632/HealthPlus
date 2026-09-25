import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import { AiChatResponse, AiConversationSummary, AiMessage, SendAiMessageRequest } from '../../models/ai-chat.models';

@Injectable({ providedIn: 'root' })
export class AiChatService {
  private readonly api = `${environment.apiUrl}/ai-chat`;

  constructor(private http: HttpClient) {}

  sendMessage(request: SendAiMessageRequest): Observable<ApiResponse<AiChatResponse>> {
    return this.http.post<ApiResponse<AiChatResponse>>(`${this.api}/messages`, request);
  }

  getConversations(): Observable<ApiResponse<AiConversationSummary[]>> {
    return this.http.get<ApiResponse<AiConversationSummary[]>>(`${this.api}/conversations`);
  }

  getMessages(conversationId: string): Observable<ApiResponse<AiMessage[]>> {
    return this.http.get<ApiResponse<AiMessage[]>>(`${this.api}/conversations/${conversationId}/messages`);
  }
}
