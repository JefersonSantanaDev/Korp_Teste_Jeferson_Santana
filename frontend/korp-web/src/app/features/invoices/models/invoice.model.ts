export type InvoiceStatus = 'Open' | 'Closed';

export interface InvoiceItem {
  productId: string;
  productCode: string;
  productDescription: string;
  quantity: number;
}

export interface InvoiceSummary {
  id: string;
  number: number;
  status: InvoiceStatus;
  createdAt: string;
  closedAt: string | null;
}

export interface Invoice extends InvoiceSummary {
  items: InvoiceItem[];
}

export interface CreateInvoiceItemRequest {
  productId: string;
  quantity: number;
}

export interface CreateInvoiceRequest {
  items: CreateInvoiceItemRequest[];
}
