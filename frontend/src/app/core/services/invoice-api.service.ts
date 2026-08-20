import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { InvoiceItem, AddInvoiceItemRequest, UpdateInvoiceItemRequest } from '../models/invoice-item';
import { Invoice } from '../models/invoice';

@Injectable({ providedIn: 'root' })
export class InvoiceApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = 'http://localhost:5007/api/invoices';

  getAll(): Observable<Invoice[]> {
    return this.http.get<Invoice[]>(this.baseUrl);
  }

  getById(id: string): Observable<Invoice> {
    return this.http.get<Invoice>(`${this.baseUrl}/${id}`);
  }

  create(): Observable<Invoice> {
    return this.http.post<Invoice>(this.baseUrl, null);
  }

  close(id: string): Observable<Invoice> {
    return this.http.post<Invoice>(`${this.baseUrl}/${id}/close`, null);
  }

  getItems(invoiceId: string): Observable<InvoiceItem[]> {
    return this.http.get<InvoiceItem[]>(`${this.baseUrl}/${invoiceId}/items`);
  }

  addItem(invoiceId: string, request: AddInvoiceItemRequest): Observable<InvoiceItem> {
    return this.http.post<InvoiceItem>(`${this.baseUrl}/${invoiceId}/items`, request);
  }

  updateItem(invoiceId: string, itemId: string, request: UpdateInvoiceItemRequest): Observable<InvoiceItem> {
    return this.http.put<InvoiceItem>(`${this.baseUrl}/${invoiceId}/items/${itemId}`, request);
  }

  deleteItem(invoiceId: string, itemId: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${invoiceId}/items/${itemId}`);
  }
}
