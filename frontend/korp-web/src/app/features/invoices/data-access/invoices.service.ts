import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { CreateInvoiceRequest, Invoice, InvoiceSummary } from '../models/invoice.model';

@Injectable({ providedIn: 'root' })
export class InvoicesService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = `${environment.billingApiBaseUrl}/invoices`;

  list(): Observable<InvoiceSummary[]> {
    return this.http.get<InvoiceSummary[]>(this.baseUrl);
  }

  getById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.baseUrl}/${id}`);
  }

  create(request: CreateInvoiceRequest): Observable<Invoice> {
    return this.http.post<Invoice>(this.baseUrl, request);
  }

  close(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/${id}/close`, {});
  }
}
