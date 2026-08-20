export type InvoiceStatus = 0 | 1;

export interface Invoice {
  id: string;
  number: string;
  status: InvoiceStatus;
  createdAt: string;
  closedAt: string | null;
}

export interface ApiErrorResponse {
  statusCode: number;
  message: string;
  traceId: string;
}
