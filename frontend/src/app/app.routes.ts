import { Routes } from '@angular/router';
import { InvoiceDetailsPage } from './pages/invoice-details/invoice-details-page';
import { InvoicesPage } from './pages/invoices/invoices-page';
import { ProductsPage } from './pages/products/products-page';

export const routes: Routes = [
  {
    path: '',
    pathMatch: 'full',
    redirectTo: 'products'
  },
  {
    path: 'products',
    component: ProductsPage,
    title: 'Produtos | Emissão NF'
  },
  {
    path: 'invoices',
    component: InvoicesPage,
    title: 'Notas Fiscais | Emissão NF'
  },
  {
    path: 'invoices/:id',
    component: InvoiceDetailsPage,
    title: 'Detalhes da Nota | Emissão NF'
  },
  {
    path: '**',
    redirectTo: 'products'
  }
];
