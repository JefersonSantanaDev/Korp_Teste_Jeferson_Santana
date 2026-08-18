import { Routes } from '@angular/router';

export const PRODUCTS_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/product-list-page/product-list-page').then((m) => m.ProductListPage),
  },
  {
    path: 'novo',
    loadComponent: () => import('./pages/product-form-page/product-form-page').then((m) => m.ProductFormPage),
  },
];
