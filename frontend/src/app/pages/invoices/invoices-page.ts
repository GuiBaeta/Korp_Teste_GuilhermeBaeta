import { DatePipe } from '@angular/common';
import { HttpErrorResponse } from '@angular/common/http';
import { ChangeDetectorRef, Component, OnInit, inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { ApiErrorResponse, Invoice, InvoiceStatus } from '../../core/models/invoice';
import { InvoiceApiService } from '../../core/services/invoice-api.service';

@Component({
  selector: 'app-invoices-page',
  imports: [
    DatePipe,
    MatButtonModule,
    MatCardModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTableModule
  ],
  templateUrl: './invoices-page.html',
  styleUrl: './invoices-page.scss'
})
export class InvoicesPage implements OnInit {
  private readonly invoiceApi = inject(InvoiceApiService);
  private readonly snackBar = inject(MatSnackBar);
  private readonly changeDetectorRef = inject(ChangeDetectorRef);

  readonly displayedColumns = ['number', 'status', 'createdAt', 'closedAt'];

  invoices: Invoice[] = [];
  loading = true;
  creating = false;
  loadError = '';

  ngOnInit(): void {
    this.loadInvoices();
  }

  loadInvoices(): void {
    this.loading = true;
    this.loadError = '';

    this.invoiceApi.getAll()
      .pipe(finalize(() => {
        this.loading = false;
        this.changeDetectorRef.markForCheck();
      }))
      .subscribe({
        next: invoices => this.invoices = invoices,
        error: error => {
          this.loadError = this.getErrorMessage(
            error,
            'Não foi possível carregar as notas fiscais. Verifique se o serviço de faturamento está disponível.'
          );
        }
      });
  }

  createInvoice(): void {
    if (this.creating) {
      return;
    }

    this.creating = true;

    this.invoiceApi.create()
      .pipe(finalize(() => {
        this.creating = false;
        this.changeDetectorRef.markForCheck();
      }))
      .subscribe({
        next: invoice => {
          this.invoices = [invoice, ...this.invoices];
          this.snackBar.open(`Nota ${invoice.number} criada com sucesso.`, 'Fechar', { duration: 3500 });
        },
        error: error => {
          this.snackBar.open(
            this.getErrorMessage(error, 'Não foi possível criar a nota fiscal.'),
            'Fechar',
            { duration: 5000 }
          );
        }
      });
  }

  statusLabel(status: InvoiceStatus): string {
    return status === 1 ? 'Fechada' : 'Aberta';
  }

  private getErrorMessage(error: unknown, fallback: string): string {
    if (error instanceof HttpErrorResponse) {
      const apiError = error.error as Partial<ApiErrorResponse> | null;
      if (apiError?.message) {
        return apiError.message;
      }
    }

    return fallback;
  }
}
