import { Component } from '@angular/core';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-public-nav',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, MatIconModule, MatButtonModule],
  templateUrl: './public-nav.component.html',
  styleUrl: './public-nav.component.scss',
})
export class PublicNavComponent {}
