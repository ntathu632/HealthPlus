import { Component, input } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { PublicNavComponent } from '../public-nav/public-nav.component';
import { PublicFooterComponent } from '../public-footer/public-footer.component';

@Component({
  selector: 'app-info-page-layout',
  standalone: true,
  imports: [MatIconModule, PublicNavComponent, PublicFooterComponent],
  templateUrl: './info-page-layout.component.html',
  styleUrl: './info-page-layout.component.scss',
})
export class InfoPageLayoutComponent {
  title = input.required<string>();
  subtitle = input<string>('');
  icon = input<string>('favorite');
}
