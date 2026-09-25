import { Injectable } from '@angular/core';
import { initializeApp, FirebaseApp } from 'firebase/app';
import { getMessaging, getToken, onMessage, Messaging } from 'firebase/messaging';
import { environment } from '../../../environments/environment';

@Injectable({ providedIn: 'root' })
export class PushNotificationService {
  private app: FirebaseApp | null = null;
  private messaging: Messaging | null = null;
  private foregroundListenerAttached = false;

  readonly isConfigured = !!environment.firebaseConfig.apiKey;
  readonly isSupported =
    typeof window !== 'undefined' && 'Notification' in window && 'serviceWorker' in navigator;

  /**
   * Xin quyền thông báo trình duyệt và lấy FCM registration token.
   * Trả về null nếu chưa cấu hình Firebase, trình duyệt không hỗ trợ, hoặc user từ chối quyền.
   */
  async requestPermissionAndGetToken(): Promise<string | null> {
    if (!this.isConfigured || !this.isSupported) return null;

    try {
      const permission = await Notification.requestPermission();
      if (permission !== 'granted') return null;

      const registration = await navigator.serviceWorker.register('/firebase-messaging-sw.js');
      const messaging = this.getOrInitMessaging();

      return await getToken(messaging, {
        vapidKey: environment.fcmVapidKey,
        serviceWorkerRegistration: registration,
      });
    } catch (err) {
      console.error('[FCM] Không lấy được token:', err);
      return null;
    }
  }

  /** Lắng nghe thông báo khi app đang mở (foreground). Gọi 1 lần lúc app khởi động. */
  listenForegroundMessages(onReceive: (title: string, body: string) => void): void {
    if (!this.isConfigured || !this.isSupported || this.foregroundListenerAttached) return;
    this.foregroundListenerAttached = true;

    const messaging = this.getOrInitMessaging();
    onMessage(messaging, (payload) => {
      onReceive(payload.notification?.title ?? 'HealthPlus', payload.notification?.body ?? '');
    });
  }

  private getOrInitMessaging(): Messaging {
    if (!this.app) this.app = initializeApp(environment.firebaseConfig);
    if (!this.messaging) this.messaging = getMessaging(this.app);
    return this.messaging;
  }
}
