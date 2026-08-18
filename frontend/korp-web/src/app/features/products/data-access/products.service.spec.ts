import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../../environments/environment';
import { Product } from '../models/product.model';
import { ProductsService } from './products.service';

describe('ProductsService', () => {
  let service: ProductsService;
  let httpMock: HttpTestingController;
  const baseUrl = `${environment.inventoryApiBaseUrl}/products`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(ProductsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('lists products from the Inventory API', () => {
    const products: Product[] = [{ id: '1', code: 'PROD-001', description: 'Teclado', stock: 10 }];
    let result: Product[] | undefined;

    service.list().subscribe((response) => (result = response));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('GET');
    req.flush(products);

    expect(result).toEqual(products);
  });

  it('creates a product against the Inventory API', () => {
    const request = { code: 'PROD-002', description: 'Mouse', stock: 5 };
    const response: Product = { id: '2', ...request };
    let result: Product | undefined;

    service.create(request).subscribe((product) => (result = product));

    const req = httpMock.expectOne(baseUrl);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(response);

    expect(result).toEqual(response);
  });
});
