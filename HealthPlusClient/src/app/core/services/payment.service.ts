import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import { Payment } from '../../models/payment.models';

@Injectable({ providedIn: 'root' })
export class PaymentService {
  private readonly api = `${environment.apiUrl}/payments`;

  constructor(private http: HttpClient) {}

  getMyPayments(): Observable<ApiResponse<Payment[]>> {
    return this.http.get<ApiResponse<Payment[]>>(`${this.api}/my`);
  }

  getMyEarnings(): Observable<ApiResponse<Payment[]>> {
    return this.http.get<ApiResponse<Payment[]>>(`${this.api}/doctor-earnings`);
  }

  pay(appointmentId: string): Observable<ApiResponse<Payment>> {
    return this.http.post<ApiResponse<Payment>>(`${this.api}/${appointmentId}/pay`, {});
  }
}
