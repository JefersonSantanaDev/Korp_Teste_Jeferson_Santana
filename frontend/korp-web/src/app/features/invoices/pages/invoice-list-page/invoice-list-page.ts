import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { formatInvoiceNumber } from '../../../../shared/utils/format-invoice-number.util';
import { toUserMessage } from '../../../../shared/utils/api-error.util';
import { InvoicesService } from '../../data-access/invoices.service';
import { InvoiceSummary } from '../../models/invoice.model';

@Component({
  selector: 'app-invoice-list-page',
  imports: [RouterLink, DatePipe],
  templateUrl: './invoice-list-page.html',
})
export class InvoiceListPage implements OnInit {
  private readonly invoicesService = inject(InvoicesService);

  protected readonly invoices = signal<InvoiceSummary[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);
  protected readonly formatNumber = formatInvoiceNumber;

  ngOnInit(): void {
    this.reload();
  }

  protected reload(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.invoicesService
      .list()
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(toUserMessage(error));
          return of<InvoiceSummary[]>([]);
        }),
        finalize(() => this.loading.set(false)),
      )
      .subscribe((invoices) => this.invoices.set(invoices));
  }
}
