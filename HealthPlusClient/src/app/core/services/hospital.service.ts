import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import { Hospital } from '../../models/hospital.models';

@Injectable({ providedIn: 'root' })
export class HospitalService {
  private readonly api = `${environment.apiUrl}/hospitals`;

  constructor(private http: HttpClient) {}

  getAll(): Observable<ApiResponse<Hospital[]>> {
    return this.http.get<ApiResponse<Hospital[]>>(this.api);
  }
}
