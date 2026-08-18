import { HttpErrorResponse } from '@angular/common/http';
import { ProblemDetails } from '../models/problem-details.model';

// Backend ProblemDetails text (title/detail/validation messages) is
// English and meant for logs/Swagger, not end users — never surface it
// directly. Always translate by status code into a fixed PT-BR message.
const FIELD_LABELS_PT: Record<string, string> = {
  Code: 'Código',
  Description: 'Descrição',
  Stock: 'Saldo',
  Quantity: 'Quantidade',
  ProductId: 'Produto',
  Items: 'Itens',
};

/** Maps an API error response into a contextual, Portuguese, user-facing message. */
export function toUserMessage(error: HttpErrorResponse): string {
  if (error.status === 0) {
    return 'Não foi possível conectar ao servidor. Verifique sua conexão e tente novamente.';
  }

  const problem = error.error as ProblemDetails | null;

  switch (error.status) {
    case 400:
      return describeValidationError(problem);
    case 404:
      return 'Registro não encontrado.';
    case 409:
      return 'Já existe um registro com os dados informados.';
    case 503:
      return 'O serviço está temporariamente indisponível. Tente novamente em instantes.';
    default:
      return 'Ocorreu um erro inesperado. Tente novamente.';
  }
}

function describeValidationError(problem: ProblemDetails | null): string {
  const firstKey = problem?.errors ? Object.keys(problem.errors)[0] : undefined;
  if (!firstKey) {
    return 'Verifique os dados informados e tente novamente.';
  }
  const label = FIELD_LABELS_PT[firstKey] ?? firstKey;
  return `Verifique o campo "${label}" e tente novamente.`;
}
