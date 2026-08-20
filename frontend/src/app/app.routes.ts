import { Routes } from '@angular/router';
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
    path: '**',
    redirectTo: 'products'
  }
];
