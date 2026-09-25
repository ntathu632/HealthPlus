import { Component } from '@angular/core';
import { RouterLink } from '@angular/router';
import { InfoPageLayoutComponent } from '../../../layout/info-page-layout/info-page-layout.component';

@Component({
  selector: 'app-terms',
  standalone: true,
  imports: [RouterLink, InfoPageLayoutComponent],
  templateUrl: './terms.component.html',
})
export class TermsComponent {}
