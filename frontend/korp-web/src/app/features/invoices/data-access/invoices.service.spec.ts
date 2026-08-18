import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { Invoice, InvoiceSummary } from '../models/invoice.model';
import { InvoicesService } from './invoices.service';

describe('InvoicesService', () => {
  let service: InvoicesService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.billingApiBaseUrl}/invoices`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(InvoicesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lists invoices from the Billing API', () => {
    const invoices: InvoiceSummary[] = [
      { id: '1', number: 1, status: 'Open', createdAt: '2026-08-18T00:00:00Z', closedAt: null },
    ];
    let result: InvoiceSummary[] | undefined;

    service.list().subscribe((response) => (result = response));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush(invoices);

    expect(result).toEqual(invoices);
  });

  it('fetches a single invoice by id', () => {
    const invoice: Invoice = {
      id: '1',
      number: 1,
      status: 'Open',
      createdAt: '2026-08-18T00:00:00Z',
      closedAt: null,
      items: [{ productId: 'p1', productCode: 'PROD-001', productDescription: 'Teclado', quantity: 2 }],
    };
    let result: Invoice | undefined;

    service.getById('1').subscribe((response) => (result = response));

    const req = httpMock.expectOne(`${baseUrl}/1`);
    expect(req.request.method).toBe('GET');
    req.flush(invoice);

    expect(result).toEqual(invoice);
  });

  it('creates an invoice against the Billing API', () => {
    const request = { items: [{ productId: 'p1', quantity: 2 }] };
    const response: Invoice = {
      id: '1',
      number: 1,
      status: 'Open',
      createdAt: '2026-08-18T00:00:00Z',
      closedAt: null,
      items: [{ productId: 'p1', productCode: 'PROD-001', productDescription: 'Teclado', quantity: 2 }],
    };
    let result: Invoice | undefined;

    service.create(request).subscribe((invoice) => (result = invoice));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(response);

    expect(result).toEqual(response);
  });
});
