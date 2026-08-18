import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { InvoicesService } from '../../data-access/invoices.service';
import { InvoiceSummary } from '../../models/invoice.model';
import { InvoiceListPage } from './invoice-list-page';

describe('InvoiceListPage', () => {
  async function setup(list: () => Observable<InvoiceSummary[]>) {
    TestBed.configureTestingModule({
      imports: [InvoiceListPage],
      providers: [provideRouter([]), { provide: InvoicesService, useValue: { list } }],
    });
    const fixture = TestBed.createComponent(InvoiceListPage);
    await fixture.whenStable();
    return fixture;
  }

  it('shows an empty state when there are no invoices', async () => {
    const fixture = await setup(() => of([]));
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Nenhuma nota cadastrada');
  });

  it('renders invoices with a 6-digit number and a PT-BR status label', async () => {
    const invoices: InvoiceSummary[] = [
      { id: '1', number: 1, status: 'Open', createdAt: '2026-08-18T00:00:00Z', closedAt: null },
    ];
    const fixture = await setup(() => of(invoices));
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('000001');
    expect(text).toContain('Aberta');
  });

  it('shows a contextual message when loading fails', async () => {
    const fixture = await setup(() => throwError(() => new HttpErrorResponse({ status: 503 })));
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('temporariamente indisponível');
  });
});
