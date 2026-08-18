import { HttpErrorResponse } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { Observable, of, throwError } from 'rxjs';
import { ProductsService } from '../../data-access/products.service';
import { Product } from '../../models/product.model';
import { ProductListPage } from './product-list-page';

describe('ProductListPage', () => {
  async function setup(list: () => Observable<Product[]>) {
    TestBed.configureTestingModule({
      imports: [ProductListPage],
      providers: [provideRouter([]), { provide: ProductsService, useValue: { list } }],
    });
    const fixture = TestBed.createComponent(ProductListPage);
    await fixture.whenStable();
    return fixture;
  }

  it('shows an empty state when there are no products', async () => {
    const fixture = await setup(() => of([]));
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('Nenhum produto cadastrado');
  });

  it('renders products returned by the service', async () => {
    const products: Product[] = [{ id: '1', code: 'PROD-001', description: 'Teclado Mecânico', stock: 10 }];
    const fixture = await setup(() => of(products));
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('PROD-001');
    expect(text).toContain('Teclado Mecânico');
    expect(text).toContain('10');
  });

  it('shows a contextual message when loading fails', async () => {
    const fixture = await setup(() => throwError(() => new HttpErrorResponse({ status: 503 })));
    const text = (fixture.nativeElement as HTMLElement).textContent ?? '';
    expect(text).toContain('temporariamente indisponível');
  });
});
