import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';

export interface ContactRequest {
  name: string;
  email: string;
  message: string;
}

@Injectable({ providedIn: 'root' })
export class ContactService {
  private readonly api = `${environment.apiUrl}/contact`;

  constructor(private http: HttpClient) {}

  send(request: ContactRequest) {
    return this.http.post<ApiResponse<null>>(this.api, request);
  }
}
