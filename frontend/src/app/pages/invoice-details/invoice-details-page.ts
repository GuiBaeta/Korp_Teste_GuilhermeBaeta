import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSelectModule } from '@angular/material/select';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { forkJoin, finalize } from 'rxjs';
import { InvoiceItem } from '../../core/models/invoice-item';
import { ApiErrorResponse, Invoice } from '../../core/models/invoice';
import { Product } from '../../core/models/product';
import { InvoiceApiService } from '../../core/services/invoice-api.service';
import { ProductApiService } from '../../core/services/product-api.service';

@Component({
  selector: 'app-invoice-details-page',
  imports: [
    DatePipe,
    FormsModule,
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSelectModule,
    MatSnackBarModule,
    MatTableModule,
    ReactiveFormsModule,
    RouterLink
  ],
  templateUrl: './invoice-details-page.html',
  styleUrl: './invoice-details-page.scss'
})
export class InvoiceDetailsPage implements OnInit {
  private readonly route = inject(ActivatedRoute);
  private readonly formBuilder = inject(FormBuilder);
  private readonly invoiceApi = inject(InvoiceApiService);
  private readonly productApi = inject(ProductApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);

  readonly displayedColumns = ['productCode', 'productDescription', 'quantity', 'actions'];
  readonly addForm = this.formBuilder.nonNullable.group({
    productId: ['', Validators.required],
    quantity: [1, [Validators.required, Validators.min(1), Validators.pattern(/^\d+$/)]]
  });

  invoiceId = '';
  invoice: Invoice | null = null;
  items: InvoiceItem[] = [];
  products: Product[] = [];
  loading = true;
  saving = false;
  closing = false;
  loadError = '';
  editingItemId: string | null = null;
  editQuantity = 1;

  get isOpen(): boolean {
    return this.invoice?.status === 0;
  }

  ngOnInit(): void {
    this.invoiceId = this.route.snapshot.paramMap.get('id') ?? '';
    this.loadDetails();
  }

  loadDetails(): void {
    if (!this.invoiceId) {
      this.loadError = 'Identificador da nota fiscal inválido.';
      this.loading = false;
      return;
    }

    this.loading = true;
    this.loadError = '';

    forkJoin({
      invoice: this.invoiceApi.getById(this.invoiceId),
      items: this.invoiceApi.getItems(this.invoiceId),
      products: this.productApi.getAll()
    })
      .pipe(finalize(() => {
        this.loading = false;
        this.changeDetectorRef.markForCheck();
      }))
      .subscribe({
        next: result => {
          this.invoice = result.invoice;
          this.items = result.items;
          this.products = result.products;
        },
        error: error => {
          this.loadError = this.getErrorMessage(error, 'Não foi possível carregar os detalhes da nota fiscal.');
        }
      });
  }

  addItem(): void {
    if (!this.isOpen || this.addForm.invalid || this.saving) {
      this.addForm.markAllAsTouched();
      return;
    }

    this.saving = true;
    this.invoiceApi.addItem(this.invoiceId, this.addForm.getRawValue())
      .pipe(finalize(() => {
        this.saving = false;
        this.changeDetectorRef.markForCheck();
      }))
      .subscribe({
        next: item => {
          this.items = [...this.items, item];
          this.addForm.reset({ productId: '', quantity: 1 });
          this.snackBar.open('Item adicionado à nota.', 'Fechar', { duration: 3000 });
        },
        error: error => this.showError(error, 'Não foi possível adicionar o item.')
      });
  }

  startEdit(item: InvoiceItem): void {
    if (!this.isOpen) return;
    this.editingItemId = item.id;
    this.editQuantity = item.quantity;
  }

  cancelEdit(): void {
    this.editingItemId = null;
  }

  updateItem(item: InvoiceItem): void {
    if (!this.isOpen || this.saving || !Number.isInteger(this.editQuantity) || this.editQuantity < 1) {
      return;
    }

    this.saving = true;
    this.invoiceApi.updateItem(this.invoiceId, item.id, { quantity: this.editQuantity })
      .pipe(finalize(() => {
        this.saving = false;
        this.changeDetectorRef.markForCheck();
      }))
      .subscribe({
        next: updated => {
          this.items = this.items.map(current => current.id === updated.id ? updated : current);
          this.editingItemId = null;
          this.snackBar.open('Quantidade atualizada.', 'Fechar', { duration: 3000 });
        },
        error: error => this.showError(error, 'Não foi possível atualizar o item.')
      });
  }

  deleteItem(item: InvoiceItem): void {
    if (!this.isOpen || this.saving) return;

    this.saving = true;
    this.invoiceApi.deleteItem(this.invoiceId, item.id)
      .pipe(finalize(() => {
        this.saving = false;
        this.changeDetectorRef.markForCheck();
      }))
      .subscribe({
        next: () => {
          this.items = this.items.filter(current => current.id !== item.id);
          this.snackBar.open('Item removido da nota.', 'Fechar', { duration: 3000 });
        },
        error: error => this.showError(error, 'Não foi possível remover o item.')
      });
  }

  closeInvoice(): void {
    if (!this.isOpen || this.items.length === 0 || this.closing) return;

    this.closing = true;
    this.invoiceApi.close(this.invoiceId)
      .pipe(finalize(() => {
        this.closing = false;
        this.changeDetectorRef.markForCheck();
      }))
      .subscribe({
        next: invoice => {
          this.invoice = invoice;
          this.editingItemId = null;
          this.snackBar.open(`Nota ${invoice.number} fechada com sucesso. Estoque atualizado.`, 'Fechar', { duration: 4500 });
        },
        error: error => {
          if (error instanceof HttpErrorResponse && error.status === 503) {
            this.snackBar.open(
              'Não foi possível concluir a nota. O serviço de estoque está temporariamente indisponível. Tente novamente.',
              'Fechar',
              { duration: 7000 }
            );
            return;
          }
          this.showError(error, 'Não foi possível fechar a nota fiscal.');
        }
      });
  }

  statusLabel(): string {
    return this.invoice?.status === 1 ? 'Fechada' : 'Aberta';
  }

  private showError(error: unknown, fallback: string): void {
    this.snackBar.open(this.getErrorMessage(error, fallback), 'Fechar', { duration: 5500 });
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as Partial<ApiErrorResponse> | null;
      if (apiError?.message) return apiError.message;
    }
    return fallback;
  }
}
