import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InfoPageLayoutComponent } from '../../../layout/info-page-layout/info-page-layout.component';

@Component({
  selector: 'app-privacy',
  standalone: true,
  imports: [RouterLink, InfoPageLayoutComponent],
  templateUrl: './privacy.component.html',
})
export class PrivacyComponent {}
