import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { InvoicesService } from '../../data-access/invoices.service';
import { Invoice } from '../../models/invoice.model';
import { InvoiceDetailPage } from './invoice-detail-page';

describe('InvoiceDetailPage', () => {
  it('renders the invoice number, status and items once loaded', async () => {
    const invoice: Invoice = {
      id: '1',
      number: 7,
      status: 'Open',
      createdAt: '2026-08-18T00:00:00Z',
      closedAt: null,
      items: [{ productId: 'p1', productCode: 'PROD-001', productDescription: 'Teclado Mecânico', quantity: 3 }],
    };

    TestBed.configureTestingModule({
      imports: [InvoiceDetailPage],
      providers: [
        { provide: InvoicesService, useValue: { getById: () => of(invoice) } },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } },
        },
      ],
    });

    const fixture = TestBed.createComponent(InvoiceDetailPage);
    await fixture.whenStable();

    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('000007');
    expect(text).toContain('Aberta');
    expect(text).toContain('PROD-001');
    expect(text).toContain('Teclado Mecânico');
  });
});
