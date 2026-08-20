import { HttpErrorResponse } from '@angular/common/http';
import { Component, OnInit, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatCardModule } from '@angular/material/card';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBar, MatSnackBarModule } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { finalize } from 'rxjs';
import { ApiErrorResponse, CreateProductRequest, Product } from '../../core/models/product';
import { ProductApiService } from '../../core/services/product-api.service';

@Component({
  selector: 'app-products-page',
  imports: [
    MatButtonModule,
    MatCardModule,
    MatFormFieldModule,
    MatInputModule,
    MatProgressSpinnerModule,
    MatSnackBarModule,
    MatTableModule,
    ReactiveFormsModule
  ],
  templateUrl: './products-page.html',
  styleUrl: './products-page.scss'
})
export class ProductsPage implements OnInit {
  private readonly formBuilder = inject(FormBuilder);
  private readonly productApi = inject(ProductApiService);
  private readonly snackBar = inject(MatSnackBar);

  readonly displayedColumns = ['code', 'description', 'stockQuantity'];
  readonly form = this.formBuilder.nonNullable.group({
    code: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', [Validators.required, Validators.maxLength(200)]],
    stockQuantity: [0, [Validators.required, Validators.min(0), Validators.pattern(/^\d+$/)]]
  });

  products: Product[] = [];
  loading = true;
  saving = false;
  loadError = '';

  ngOnInit(): void {
    this.loadProducts();
  }

  loadProducts(): void {
    this.loading = true;
    this.loadError = '';

    this.productApi.getAll()
      .pipe(finalize(() => this.loading = false))
      .subscribe({
        next: products => this.products = products,
        error: error => {
          this.loadError = this.getErrorMessage(
            error,
            'Não foi possível carregar os produtos. Verifique se o serviço de estoque está disponível.'
          );
        }
      });
  }

  createProduct(): void {
    if (this.form.invalid || this.saving) {
      this.form.markAllAsTouched();
      return;
    }

    const value = this.form.getRawValue();
    const request: CreateProductRequest = {
      code: value.code.trim(),
      description: value.description.trim(),
      stockQuantity: value.stockQuantity
    };

    if (!request.code || !request.description) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving = true;

    this.productApi.create(request)
      .pipe(finalize(() => this.saving = false))
      .subscribe({
        next: product => {
          this.products = [...this.products, product];
          this.form.reset({ code: '', description: '', stockQuantity: 0 });
          this.snackBar.open('Produto cadastrado com sucesso.', 'Fechar', { duration: 3500 });
        },
        error: error => {
          this.snackBar.open(
            this.getErrorMessage(error, 'Não foi possível cadastrar o produto.'),
            'Fechar',
            { duration: 5000 }
          );
        }
      });
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
