export interface InvoiceItem {
  id: string;
  invoiceId: string;
  productId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface AddInvoiceItemRequest {
  productId: string;
  quantity: number;
}

export interface UpdateInvoiceItemRequest {
  quantity: number;
}
