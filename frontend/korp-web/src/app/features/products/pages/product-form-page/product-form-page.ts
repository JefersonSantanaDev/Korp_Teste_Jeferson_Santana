import { HttpErrorResponse } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { NonNullableFormBuilder, ReactiveFormsModule, ValidationErrors, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { EMPTY, catchError } from 'rxjs';
import { toUserMessage } from '../../../../shared/utils/api-error.util';
import { ProductsService } from '../../data-access/products.service';
import { CreateProductRequest } from '../../models/product.model';

type ProductFieldName = 'code' | 'description' | 'stock';

const FIELD_LABELS: Record<ProductFieldName, string> = {
  code: 'Código',
  description: 'Descrição',
  stock: 'Saldo',
};

@Component({
  selector: 'app-product-form-page',
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './product-form-page.html',
})
export class ProductFormPage {
  private readonly formBuilder = inject(NonNullableFormBuilder);
  private readonly productsService = inject(ProductsService);
  private readonly router = inject(Router);

  protected readonly saving = signal(false);
  protected readonly errorMessage = signal<string | null>(null);

  protected readonly form = this.formBuilder.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', [Validators.required, Validators.maxLength(200)]],
    stock: [0, [Validators.required, Validators.min(0)]],
  });

  protected fieldError(name: ProductFieldName): string | null {
    const control = this.form.controls[name];
    if (!control.touched || !control.errors) {
      return null;
    }
    return describeError(name, control.errors);
  }

  protected submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);
    this.errorMessage.set(null);

    const value = this.form.getRawValue();
    const request: CreateProductRequest = {
      code: value.code.trim(),
      description: value.description.trim(),
      stock: value.stock,
    };

    this.productsService
      .create(request)
      .pipe(
        catchError((error: HttpErrorResponse) => {
          this.errorMessage.set(toUserMessage(error));
          this.saving.set(false);
          return EMPTY;
        }),
      )
      .subscribe(() => this.router.navigate(['/produtos']));
  }
}

function describeError(name: ProductFieldName, errors: ValidationErrors): string {
  if (errors['required']) {
    return `${FIELD_LABELS[name]} é obrigatório.`;
  }
  if (errors['maxlength']) {
    return `Máximo de ${errors['maxlength'].requiredLength} caracteres.`;
  }
  if (errors['min']) {
    return 'O saldo não pode ser negativo.';
  }
  return 'Valor inválido.';
}
