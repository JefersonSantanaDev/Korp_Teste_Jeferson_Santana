import { Routes } from '@angular/router';

export const routes: Routes = [
  { path: '', redirectTo: 'produtos', pathMatch: 'full' },
  {
    path: 'produtos',
    loadChildren: () => import('./features/products/products.routes').then((m) => m.PRODUCTS_ROUTES),
  },
  {
    path: 'notas',
    loadChildren: () => import('./features/invoices/invoices.routes').then((m) => m.INVOICES_ROUTES),
  },
];
