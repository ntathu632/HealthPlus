import { Component, signal, ViewChild, ElementRef, AfterViewChecked } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { AiChatService } from '../../core/services/ai-chat.service';
import { AiMessage } from '../../models/ai-chat.models';

@Component({
  selector: 'app-ai-chat',
  standalone: true,
  imports: [FormsModule, MatIconModule, MatButtonModule, MatProgressSpinnerModule],
  templateUrl: './ai-chat.component.html',
  styleUrl: './ai-chat.component.scss',
})
export class AiChatComponent implements AfterViewChecked {
  @ViewChild('scrollAnchor') private scrollAnchor?: ElementRef<HTMLDivElement>;

  messages = signal<AiMessage[]>([]);
  sending = signal(false);
  draft = '';
  private conversationId: string | null = null;
  private shouldScroll = false;

  readonly suggestions = [
    'Tôi hay bị đau đầu vào buổi chiều, có đáng lo không?',
    'Chế độ ăn nào tốt cho người huyết áp cao?',
    'Trẻ nhỏ sốt bao nhiêu độ thì cần đi khám?',
  ];

  constructor(private svc: AiChatService) {}

  ngAfterViewChecked(): void {
    if (this.shouldScroll) {
      this.scrollAnchor?.nativeElement.scrollIntoView({ behavior: 'smooth' });
      this.shouldScroll = false;
    }
  }

  send(text?: string): void {
    const content = (text ?? this.draft).trim();
    if (!content || this.sending()) return;

    const userMessage: AiMessage = {
      id: `local-${Date.now()}`,
      role: 'user',
      content,
      createdAt: new Date().toISOString(),
    };
    this.messages.update(list => [...list, userMessage]);
    this.draft = '';
    this.sending.set(true);
    this.shouldScroll = true;

    this.svc.sendMessage({ conversationId: this.conversationId ?? undefined, message: content }).subscribe({
      next: res => {
        this.conversationId = res.data.conversationId;
        this.messages.update(list => [...list, res.data.reply]);
        this.sending.set(false);
        this.shouldScroll = true;
      },
      error: () => {
        this.messages.update(list => [...list, {
          id: `local-error-${Date.now()}`,
          role: 'assistant',
          content: 'Xin lỗi, đã có lỗi xảy ra. Vui lòng thử lại.',
          createdAt: new Date().toISOString(),
        }]);
        this.sending.set(false);
        this.shouldScroll = true;
      },
    });
  }
}
