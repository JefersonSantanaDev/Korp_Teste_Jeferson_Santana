import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { catchError, finalize, of } from 'rxjs';
import { toUserMessage } from '../../../../shared/utils/api-error.util';
import { ProductsService } from '../../data-access/products.service';
import { Product } from '../../models/product.model';

@Component({
  selector: 'app-product-list-page',
  imports: [RouterLink],
  templateUrl: './product-list-page.html',
})
export class ProductListPage implements OnInit {
  private readonly productsService = inject(ProductsService);

  protected readonly products = signal<Product[]>([]);
  protected readonly loading = signal(true);
  protected readonly errorMessage = signal<string | null>(null);

  ngOnInit(): void {
    this.reload();
  }

  protected reload(): void {
    this.loading.set(true);
    this.errorMessage.set(null);

    this.productsService
      .list()
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(toUserMessage(error));
          return of<Product[]>([]);
        }),
        finalize(() => this.loading.set(false)),
      )
      .subscribe((products) => this.products.set(products));
  }
}
