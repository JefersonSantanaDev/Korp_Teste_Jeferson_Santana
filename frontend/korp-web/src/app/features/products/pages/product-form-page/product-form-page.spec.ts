import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ProductsService } from '../../data-access/products.service';
import { ProductFormPage } from './product-form-page';

describe('ProductFormPage', () => {
  async function setup(create: ReturnType<typeof vi.fn>) {
    TestBed.configureTestingModule({
      imports: [ProductFormPage],
      providers: [provideRouter([]), { provide: ProductsService, useValue: { create } }],
    });
    const fixture = TestBed.createComponent(ProductFormPage);
    await fixture.whenStable();
    return fixture;
  }

  it('does not submit and marks fields as touched when the form is invalid', async () => {
    const create = vi.fn();
    const fixture = await setup(create);
    const component = fixture.componentInstance;

    component['submit']();

    expect(create).not.toHaveBeenCalled();
    expect(component['form'].controls.code.touched).toBe(true);
  });

  it('submits trimmed values and navigates back to the list on success', async () => {
    const create = vi.fn(() => of({ id: '1', code: 'PROD-001', description: 'Teclado', stock: 10 }));
    const fixture = await setup(create);
    const component = fixture.componentInstance;
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate');

    component['form'].setValue({ code: '  prod-001  ', description: '  Teclado  ', stock: 10 });
    component['submit']();

    expect(create).toHaveBeenCalledWith({ code: 'prod-001', description: 'Teclado', stock: 10 });
    expect(navigateSpy).toHaveBeenCalledWith(['/produtos']);
  });

  it('shows a contextual PT-BR error and re-enables the form when the API rejects the request', async () => {
    const create = vi.fn(() =>
      throwError(() => new HttpErrorResponse({ status: 409, error: { detail: 'A product with the provided code already exists.' } })),
    );
    const fixture = await setup(create);
    const component = fixture.componentInstance;

    component['form'].setValue({ code: 'PROD-001', description: 'Teclado', stock: 10 });
    component['submit']();
    await fixture.whenStable();

    expect(component['errorMessage']()).toBe('Já existe um registro com os dados informados.');
    expect(component['saving']()).toBe(false);
  });
});
