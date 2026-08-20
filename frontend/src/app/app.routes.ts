import { Routes } from '@angular/router';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products'
  },
  {
    path: 'products',
    loadComponent: () => import('./pages/products/products-page').then(component => component.ProductsPage),
    title: 'Produtos | Emissão NF'
  },
  {
    path: 'invoices',
    loadComponent: () => import('./pages/invoices/invoices-page').then(component => component.InvoicesPage),
    title: 'Notas Fiscais | Emissão NF'
  },
  {
    path: 'invoices/:id',
    loadComponent: () => import('./pages/invoice-details/invoice-details-page').then(component => component.InvoiceDetailsPage),
    title: 'Detalhes da Nota | Emissão NF'
  },
  {
    path: '**',
    redirectTo: 'products'
  }
];
