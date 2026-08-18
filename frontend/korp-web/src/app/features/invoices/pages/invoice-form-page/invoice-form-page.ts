import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { EMPTY, catchError, finalize, of } from 'rxjs';
import { ProductsService } from '../../../products/data-access/products.service';
import { Product } from '../../../products/models/product.model';
import { toUserMessage } from '../../../../shared/utils/api-error.util';
import { InvoicesService } from '../../data-access/invoices.service';
import { CreateInvoiceRequest } from '../../models/invoice.model';

interface InvoiceItemDraft {
  productId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

@Component({
  selector: 'app-invoice-form-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './invoice-form-page.html',
})
export class InvoiceFormPage implements OnInit {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly productsService = inject(ProductsService);
  private readonly invoicesService = inject(InvoicesService);
  private readonly router = inject(Router);

  protected readonly products = signal<Product[]>([]);
  protected readonly productsLoading = signal(true);
  protected readonly productsErrorMessage = signal<string | null>(null);

  protected readonly items = signal<InvoiceItemDraft[]>([]);
  protected readonly addItemError = signal<string | null>(null);

  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly addItemForm = this.formBuilder.group({
    productId: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(1)]],
  });

  ngOnInit(): void {
    this.productsService
      .list()
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.productsErrorMessage.set(toUserMessage(error));
          return of<Product[]>([]);
        }),
        finalize(() => this.productsLoading.set(false)),
      )
      .subscribe((products) => this.products.set(products));
  }

  protected addItem(): void {
    this.addItemError.set(null);

    if (this.addItemForm.invalid) {
      this.addItemForm.markAllAsTouched();
      return;
    }

    const { productId, quantity } = this.addItemForm.getRawValue();

    if (this.items().some((item) => item.productId === productId)) {
      this.addItemError.set('Esse produto já foi adicionado à nota.');
      return;
    }

    const product = this.products().find((candidate) => candidate.id === productId);
    if (!product) {
      return;
    }

    this.items.update((items) => [
      ...items,
      {
        productId: product.id,
        productCode: product.code,
        productDescription: product.description,
        quantity,
      },
    ]);

    this.addItemForm.reset({ productId: '', quantity: 1 });
  }

  protected removeItem(productId: string): void {
    this.items.update((items) => items.filter((item) => item.productId !== productId));
  }

  protected submit(): void {
    if (this.items().length === 0 || this.saving()) {
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const request: CreateInvoiceRequest = {
      items: this.items().map(({ productId, quantity }) => ({ productId, quantity })),
    };

    this.invoicesService
      .create(request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(toUserMessage(error));
          this.saving.set(false);
          return EMPTY;
        }),
      )
      .subscribe((invoice) => this.router.navigate(['/notas', invoice.id]));
  }
}
