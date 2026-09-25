import { Component, output } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatMenuModule } from '@angular/material/menu';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../core/auth/auth.service';

@Component({
    templateUrl: './header.component.html',
    styleUrl: './header.component.scss',
    selector: 'app-header',
  standalone: true,
  imports: [MatIconModule, MatButtonModule, MatMenuModule, RouterLink]
})
export class HeaderComponent {
  toggleSidenav = output<void>();
  constructor(public auth: AuthService) {}
  initials(): string {
    const name = this.auth.currentUserName() ?? '';
    return name.split(' ').map(w => w[0]).slice(0, 2).join('').toUpperCase();
  }
}
