import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-public-footer',
  standalone: true,
  imports: [RouterLink, MatIconModule],
  templateUrl: './public-footer.component.html',
  styleUrl: './public-footer.component.scss',
})
export class PublicFooterComponent {}
