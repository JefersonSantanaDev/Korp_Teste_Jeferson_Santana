import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of, throwError } from 'rxjs';
import { InvoicesService } from '../../data-access/invoices.service';
import { Invoice } from '../../models/invoice.model';
import { InvoiceDetailPage } from './invoice-detail-page';

describe('InvoiceDetailPage', () => {
  const openInvoice: Invoice = {
    id: '1',
    number: 7,
    status: 'Open',
    createdAt: '2026-08-18T00:00:00Z',
    closedAt: null,
    items: [{ productId: 'p1', productCode: 'PROD-001', productDescription: 'Teclado Mecânico', quantity: 3 }],
  };

  async function setup(invoicesService: Partial<InvoicesService>) {
    TestBed.configureTestingModule({
      imports: [InvoiceDetailPage],
      providers: [
        { provide: InvoicesService, useValue: invoicesService },
        {
          provide: ActivatedRoute,
          useValue: { snapshot: { paramMap: convertToParamMap({ id: '1' }) } },
        },
      ],
    });

    const fixture = TestBed.createComponent(InvoiceDetailPage);
    await fixture.whenStable();
    return fixture;
  }

  it('renders the invoice number, status and items once loaded', async () => {
    const fixture = await setup({ getById: () => of(openInvoice) });
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('000007');
    expect(text).toContain('Aberta');
    expect(text).toContain('PROD-001');
    expect(text).toContain('Teclado Mecânico');
  });

  it('shows the print button only while the invoice is open', async () => {
    const fixture = await setup({ getById: () => of(openInvoice) });
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Imprimir nota');
  });

  it('hides the print button once the invoice is closed', async () => {
    const closed: Invoice = { ...openInvoice, status: 'Closed', closedAt: '2026-08-18T01:00:00Z' };
    const fixture = await setup({ getById: () => of(closed) });
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).not.toContain('Imprimir nota');
  });

  it('closes the invoice, updates the view, and triggers window.print()', async () => {
    const closed: Invoice = { ...openInvoice, status: 'Closed', closedAt: '2026-08-18T01:00:00Z' };
    const printSpy = vi.fn();
    vi.stubGlobal('print', printSpy);

    const fixture = await setup({ getById: () => of(openInvoice), close: () => of(closed) });
    const component = fixture.componentInstance;

    component['close']();
    await new Promise((resolve) => setTimeout(resolve, 10));
    await fixture.whenStable();

    expect(component['invoice']()?.status).toBe('Closed');
    expect(printSpy).toHaveBeenCalled();

    vi.unstubAllGlobals();
  });

  it('shows a contextual error and keeps the invoice open when closing fails', async () => {
    const fixture = await setup({
      getById: () => of(openInvoice),
      close: () => throwError(() => new HttpErrorResponse({ status: 409 })),
    });
    const component = fixture.componentInstance;

    component['close']();
    await fixture.whenStable();

    expect(component['invoice']()?.status).toBe('Open');
    expect(component['closeErrorMessage']()).toBeTruthy();
    expect(component['closing']()).toBe(false);
  });
});
