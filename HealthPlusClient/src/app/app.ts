import { Component, OnInit, inject } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { PushNotificationService } from './core/services/push-notification.service';

@Component({
    templateUrl: './app.html',
    styleUrl: './app.scss',
    selector: 'app-root',
  standalone: true,
  imports: [RouterOutlet]
})
export class App implements OnInit {
  private readonly pushService = inject(PushNotificationService);

  ngOnInit(): void {
    this.pushService.listenForegroundMessages((title, body) => {
      if (Notification.permission === 'granted') {
        new Notification(title, { body });
      }
    });
  }
}
