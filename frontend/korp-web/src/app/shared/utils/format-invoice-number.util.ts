/** Displays the sequential invoice number as a fixed 6-digit string, e.g. 1 -> "000001". */
export function formatInvoiceNumber(number: number): string {
  return number.toString().padStart(6, '0');
}
