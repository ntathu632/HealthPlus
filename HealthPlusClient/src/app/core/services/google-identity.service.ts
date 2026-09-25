import { Injectable } from '@angular/core';
import { environment } from '../../../environments/environment';

declare const google: any;

// Bọc Google Identity Services (script tải qua thẻ <script> trong index.html, không phải npm package)
// để login/register component không phải tự lo việc chờ script load + khởi tạo.
@Injectable({ providedIn: 'root' })
export class GoogleIdentityService {
  readonly isConfigured = !!environment.googleClientId;

  private initialized = false;

  renderButton(container: HTMLElement, onCredential: (idToken: string) => void): void {
    if (!this.isConfigured) return;
    this.whenReady(() => {
      if (!this.initialized) {
        google.accounts.id.initialize({
          client_id: environment.googleClientId,
          callback: (response: { credential: string }) => onCredential(response.credential),
        });
        this.initialized = true;
      }
      google.accounts.id.renderButton(container, {
        type: 'icon',
        shape: 'circle',
        theme: 'outline',
        size: 'large',
      });
    });
  }

  private whenReady(cb: () => void, attemptsLeft = 20): void {
    if (typeof google !== 'undefined' && google.accounts?.id) {
      cb();
      return;
    }
    if (attemptsLeft <= 0) return;
    setTimeout(() => this.whenReady(cb, attemptsLeft - 1), 250);
  }
}
