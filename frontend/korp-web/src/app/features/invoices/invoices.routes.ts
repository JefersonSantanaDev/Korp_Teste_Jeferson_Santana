import { Routes } from '@angular/router';

export const INVOICES_ROUTES: Routes = [
  {
    path: '',
    loadComponent: () => import('./pages/invoice-list-page/invoice-list-page').then((m) => m.InvoiceListPage),
  },
  {
    path: 'nova',
    loadComponent: () => import('./pages/invoice-form-page/invoice-form-page').then((m) => m.InvoiceFormPage),
  },
  {
    path: ':id',
    loadComponent: () => import('./pages/invoice-detail-page/invoice-detail-page').then((m) => m.InvoiceDetailPage),
  },
];
