import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from '../../models/api.models';
import { PatientSummary, PatientProfile } from '../../models/doctor.models';
import { MedicalHistory, CreateMedicalHistoryRequest } from '../../models/medical-history.models';
import {
  Prescription, PrescriptionItem,
  CreatePrescriptionRequest, CreatePrescriptionItemRequest, UpdatePrescriptionItemRequest
} from '../../models/prescription.models';
import { Vaccine, CreateVaccineRequest } from '../../models/vaccine.models';

@Injectable({ providedIn: 'root' })
export class DoctorService {
  private readonly api = `${environment.apiUrl}/doctor`;

  constructor(private http: HttpClient) {}

  getMyPatients(): Observable<ApiResponse<PatientSummary[]>> {
    return this.http.get<ApiResponse<PatientSummary[]>>(`${this.api}/patients`);
  }

  getPatientProfile(patientId: string): Observable<ApiResponse<PatientProfile>> {
    return this.http.get<ApiResponse<PatientProfile>>(`${this.api}/patients/${patientId}`);
  }

  addMedicalHistory(patientId: string, request: CreateMedicalHistoryRequest): Observable<ApiResponse<MedicalHistory>> {
    return this.http.post<ApiResponse<MedicalHistory>>(`${this.api}/patients/${patientId}/medical-history`, request);
  }

  updateMedicalHistory(patientId: string, id: string, request: CreateMedicalHistoryRequest): Observable<ApiResponse<MedicalHistory>> {
    return this.http.put<ApiResponse<MedicalHistory>>(`${this.api}/patients/${patientId}/medical-history/${id}`, request);
  }

  deleteMedicalHistory(patientId: string, id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/patients/${patientId}/medical-history/${id}`);
  }

  addVaccine(patientId: string, request: CreateVaccineRequest): Observable<ApiResponse<Vaccine>> {
    return this.http.post<ApiResponse<Vaccine>>(`${this.api}/patients/${patientId}/vaccines`, request);
  }

  updateVaccine(patientId: string, id: string, request: CreateVaccineRequest): Observable<ApiResponse<Vaccine>> {
    return this.http.put<ApiResponse<Vaccine>>(`${this.api}/patients/${patientId}/vaccines/${id}`, request);
  }

  deleteVaccine(patientId: string, id: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/patients/${patientId}/vaccines/${id}`);
  }

  createPrescription(patientId: string, request: CreatePrescriptionRequest): Observable<ApiResponse<Prescription>> {
    return this.http.post<ApiResponse<Prescription>>(`${this.api}/patients/${patientId}/prescriptions`, request);
  }

  deletePrescription(patientId: string, prescriptionId: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/patients/${patientId}/prescriptions/${prescriptionId}`);
  }

  addPrescriptionItem(patientId: string, prescriptionId: string, request: CreatePrescriptionItemRequest): Observable<ApiResponse<PrescriptionItem>> {
    return this.http.post<ApiResponse<PrescriptionItem>>(`${this.api}/patients/${patientId}/prescriptions/${prescriptionId}/items`, request);
  }

  updatePrescriptionItem(patientId: string, itemId: string, request: UpdatePrescriptionItemRequest): Observable<ApiResponse<PrescriptionItem>> {
    return this.http.put<ApiResponse<PrescriptionItem>>(`${this.api}/patients/${patientId}/prescriptions/items/${itemId}`, request);
  }

  deletePrescriptionItem(patientId: string, itemId: string): Observable<ApiResponse<null>> {
    return this.http.delete<ApiResponse<null>>(`${this.api}/patients/${patientId}/prescriptions/items/${itemId}`);
  }
}
