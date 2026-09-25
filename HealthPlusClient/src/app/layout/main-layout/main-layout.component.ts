import { Component, signal, ViewChild } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { BreakpointObserver } from '@angular/cdk/layout';
import { SidebarComponent } from '../sidebar/sidebar.component';
import { HeaderComponent } from '../header/header.component';

@Component({
    templateUrl: './main-layout.component.html',
    styleUrl: './main-layout.component.scss',
    selector: 'app-main-layout',
  standalone: true,
  imports: [RouterOutlet, MatSidenavModule, SidebarComponent, HeaderComponent]
})
export class MainLayoutComponent {
  isMobile = signal(false);

  constructor(private breakpointObserver: BreakpointObserver) {
    // Chỉ coi là mobile khi màn hình thực sự nhỏ (< 768px)
    this.breakpointObserver
      .observe(['(max-width: 767px)'])
      .subscribe(result => this.isMobile.set(result.matches));
  }

  onNavClick(sidenav: MatSidenav): void {
    // Chỉ đóng sidebar khi đang ở chế độ overlay (mobile)
    if (this.isMobile()) {
      sidenav.close();
    }
  }
}
