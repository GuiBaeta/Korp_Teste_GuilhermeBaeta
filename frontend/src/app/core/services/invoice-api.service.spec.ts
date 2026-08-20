import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { AddInvoiceItemRequest, InvoiceItem, UpdateInvoiceItemRequest } from '../models/invoice-item';
import { Invoice } from '../models/invoice';
import { InvoiceApiService } from './invoice-api.service';

describe('InvoiceApiService', () => {
  let service: InvoiceApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(InvoiceApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('loads invoices with GET', () => {
    const invoices: Invoice[] = [createInvoice()];
    let response: Invoice[] | undefined;

    service.getAll().subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne('http://localhost:5007/api/invoices');
    expect(request.request.method).toBe('GET');
    request.flush(invoices);

    expect(response).toEqual(invoices);
  });

  it('loads one invoice by id', () => {
    const invoice = createInvoice();
    let response: Invoice | undefined;

    service.getById(invoice.id).subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne(`http://localhost:5007/api/invoices/${invoice.id}`);
    expect(request.request.method).toBe('GET');
    request.flush(invoice);

    expect(response).toEqual(invoice);
  });

  it('creates an invoice with POST', () => {
    const invoice = createInvoice();
    let response: Invoice | undefined;

    service.create().subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne('http://localhost:5007/api/invoices');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(invoice);

    expect(response).toEqual(invoice);
  });

  it('closes an invoice with POST', () => {
    const invoice = { ...createInvoice(), status: 1 as const, closedAt: '2026-08-20T10:30:00Z' };
    let response: Invoice | undefined;

    service.close(invoice.id).subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne(`http://localhost:5007/api/invoices/${invoice.id}/close`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toBeNull();
    request.flush(invoice);

    expect(response).toEqual(invoice);
  });

  it('loads invoice items with GET', () => {
    const invoiceId = 'invoice-1';
    const items: InvoiceItem[] = [createItem(invoiceId)];
    let response: InvoiceItem[] | undefined;

    service.getItems(invoiceId).subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne(`http://localhost:5007/api/invoices/${invoiceId}/items`);
    expect(request.request.method).toBe('GET');
    request.flush(items);

    expect(response).toEqual(items);
  });

  it('adds an invoice item with POST', () => {
    const invoiceId = 'invoice-1';
    const payload: AddInvoiceItemRequest = {
      productId: 'product-1',
      quantity: 2
    };
    const item = createItem(invoiceId);
    let response: InvoiceItem | undefined;

    service.addItem(invoiceId, payload).subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne(`http://localhost:5007/api/invoices/${invoiceId}/items`);
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush(item);

    expect(response).toEqual(item);
  });

  it('updates an invoice item with PUT', () => {
    const invoiceId = 'invoice-1';
    const itemId = 'item-1';
    const payload: UpdateInvoiceItemRequest = { quantity: 5 };
    const item = { ...createItem(invoiceId), id: itemId, quantity: payload.quantity };
    let response: InvoiceItem | undefined;

    service.updateItem(invoiceId, itemId, payload).subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne(
      `http://localhost:5007/api/invoices/${invoiceId}/items/${itemId}`
    );
    expect(request.request.method).toBe('PUT');
    expect(request.request.body).toEqual(payload);
    request.flush(item);

    expect(response).toEqual(item);
  });

  it('deletes an invoice item with DELETE', () => {
    const invoiceId = 'invoice-1';
    const itemId = 'item-1';
    let completed = false;

    service.deleteItem(invoiceId, itemId).subscribe({
      complete: () => {
        completed = true;
      }
    });

    const request = httpTesting.expectOne(
      `http://localhost:5007/api/invoices/${invoiceId}/items/${itemId}`
    );
    expect(request.request.method).toBe('DELETE');
    request.flush(null);

    expect(completed).toBe(true);
  });
});

function createInvoice(): Invoice {
  return {
    id: 'invoice-1',
    number: 'NF-2026-000001',
    status: 0,
    createdAt: '2026-08-20T10:00:00Z',
    closedAt: null
  };
}

function createItem(invoiceId: string): InvoiceItem {
  return {
    id: 'item-1',
    invoiceId,
    productId: 'product-1',
    productCode: 'PROD-001',
    productDescription: 'Produto de teste',
    quantity: 2
  };
}
