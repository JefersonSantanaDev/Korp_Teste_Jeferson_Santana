import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { of } from 'rxjs';
import { ProductsService } from '../../../products/data-access/products.service';
import { Product } from '../../../products/models/product.model';
import { InvoicesService } from '../../data-access/invoices.service';
import { InvoiceFormPage } from './invoice-form-page';

describe('InvoiceFormPage', () => {
  const products: Product[] = [
    { id: 'p1', code: 'PROD-001', description: 'Teclado', stock: 10 },
    { id: 'p2', code: 'PROD-002', description: 'Mouse', stock: 5 },
  ];

  async function setup(create: ReturnType<typeof vi.fn>) {
    TestBed.configureTestingModule({
      imports: [InvoiceFormPage],
      providers: [
        provideRouter([]),
        { provide: ProductsService, useValue: { list: () => of(products) } },
        { provide: InvoicesService, useValue: { create } },
      ],
    });
    const fixture = TestBed.createComponent(InvoiceFormPage);
    await fixture.whenStable();
    return fixture;
  }

  it('does not submit when there are no items', async () => {
    const create = vi.fn();
    const fixture = await setup(create);

    fixture.componentInstance['submit']();

    expect(create).not.toHaveBeenCalled();
  });

  it('adds an item picked from the loaded products and allows removing it', async () => {
    const fixture = await setup(vi.fn());
    const component = fixture.componentInstance;

    component['addItemForm'].setValue({ productId: 'p1', quantity: 2 });
    component['addItem']();

    expect(component['items']()).toEqual([
      { productId: 'p1', productCode: 'PROD-001', productDescription: 'Teclado', quantity: 2 },
    ]);

    component['removeItem']('p1');
    expect(component['items']()).toEqual([]);
  });

  it('rejects adding the same product twice', async () => {
    const fixture = await setup(vi.fn());
    const component = fixture.componentInstance;

    component['addItemForm'].setValue({ productId: 'p1', quantity: 1 });
    component['addItem']();
    component['addItemForm'].setValue({ productId: 'p1', quantity: 3 });
    component['addItem']();

    expect(component['items']().length).toBe(1);
    expect(component['addItemError']()).toContain('já foi adicionado');
  });

  it('submits the accumulated items and navigates to the created invoice', async () => {
    const created = {
      id: 'inv-1',
      number: 1,
      status: 'Open',
      createdAt: '2026-08-18T00:00:00Z',
      closedAt: null,
      items: [],
    };
    const create = vi.fn(() => of(created));
    const fixture = await setup(create);
    const component = fixture.componentInstance;
    const router = TestBed.inject(Router);
    const navigateSpy = vi.spyOn(router, 'navigate');

    component['addItemForm'].setValue({ productId: 'p2', quantity: 1 });
    component['addItem']();
    component['submit']();

    expect(create).toHaveBeenCalledWith({ items: [{ productId: 'p2', quantity: 1 }] });
    expect(navigateSpy).toHaveBeenCalledWith(['/notas', 'inv-1']);
  });
});
