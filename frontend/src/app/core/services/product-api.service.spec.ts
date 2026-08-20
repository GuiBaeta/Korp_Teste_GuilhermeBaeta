import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { afterEach, beforeEach, describe, expect, it } from 'vitest';
import { CreateProductRequest, Product } from '../models/product';
import { ProductApiService } from './product-api.service';

describe('ProductApiService', () => {
  let service: ProductApiService;
  let httpTesting: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(ProductApiService);
    httpTesting = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpTesting.verify();
  });

  it('loads all products with GET', () => {
    const products: Product[] = [
      {
        id: 'product-1',
        code: 'PROD-001',
        description: 'Produto de teste',
        stockQuantity: 8,
        createdAt: '2026-08-20T10:00:00Z',
        updatedAt: '2026-08-20T10:00:00Z'
      }
    ];
    let response: Product[] | undefined;

    service.getAll().subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne('http://localhost:5173/api/products');
    expect(request.request.method).toBe('GET');
    request.flush(products);

    expect(response).toEqual(products);
  });

  it('creates a product with POST', () => {
    const payload: CreateProductRequest = {
      code: 'PROD-002',
      description: 'Novo produto',
      stockQuantity: 12
    };
    const created: Product = {
      id: 'product-2',
      ...payload,
      createdAt: '2026-08-20T10:05:00Z',
      updatedAt: '2026-08-20T10:05:00Z'
    };
    let response: Product | undefined;

    service.create(payload).subscribe(value => {
      response = value;
    });

    const request = httpTesting.expectOne('http://localhost:5173/api/products');
    expect(request.request.method).toBe('POST');
    expect(request.request.body).toEqual(payload);
    request.flush(created);

    expect(response).toEqual(created);
  });
});
