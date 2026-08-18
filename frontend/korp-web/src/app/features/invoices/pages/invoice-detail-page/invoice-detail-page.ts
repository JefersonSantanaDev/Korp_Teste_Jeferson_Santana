import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { EMPTY, catchError, finalize, of } from 'rxjs';
import { toUserMessage } from '../../../../shared/utils/api-error.util';
import { formatInvoiceNumber } from '../../../../shared/utils/format-invoice-number.util';
import { InvoicesService } from '../../data-access/invoices.service';
import { Invoice } from '../../models/invoice.model';

@Component({
  selector: 'app-invoice-detail-page',
  imports: [DatePipe],
  templateUrl: './invoice-detail-page.html',
})
export class InvoiceDetailPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly invoicesService = inject(InvoicesService);

  protected readonly invoice = signal<Invoice | null>(null);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly closing = signal(false);
  protected readonly closeErrorMessage = signal<string | null>(null);

  protected readonly formatNumber = formatInvoiceNumber;

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (!id) {
      this.errorMessage.set('Nota inválida.');
      this.loading.set(false);
      return;
    }

    this.invoicesService
      .getById(id)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(toUserMessage(error));
          return of<Invoice | null>(null);
        }),
        finalize(() => this.loading.set(false)),
      )
      .subscribe((invoice) => this.invoice.set(invoice));
  }

  protected close(): void {
    const current = this.invoice();
    if (!current || this.closing()) {
      return;
    }

    this.closing.set(true);
    this.closeErrorMessage.set(null);

    this.invoicesService
      .close(current.id)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.closeErrorMessage.set(toUserMessage(error));
          this.closing.set(false);
          return EMPTY;
        }),
      )
      .subscribe((closedInvoice) => {
        this.invoice.set(closedInvoice);
        this.closing.set(false);
        // Let Angular flush the Open -> Closed view update before the browser
        // snapshots the page for printing.
        setTimeout(() => window.print(), 0);
      });
  }
}
