import { HttpErrorResponse } from '@angular/common/http';
import { toUserMessage } from './api-error.util';

describe('toUserMessage', () => {
  it('describes a network failure when the request never reached the server', () => {
    const error = new HttpErrorResponse({ status: 0 });
    expect(toUserMessage(error)).toContain('conectar ao servidor');
  });

  it('translates the first invalid field into a PT-BR message on 400, never the raw backend text', () => {
    const error = new HttpErrorResponse({
      status: 400,
      error: { errors: { Code: ['The Code field is required.'] } },
    });
    expect(toUserMessage(error)).toBe('Verifique o campo "Código" e tente novamente.');
  });

  it('never surfaces the raw (English) ProblemDetails detail on 409', () => {
    const error = new HttpErrorResponse({
      status: 409,
      error: { detail: 'A product with the provided code already exists.' },
    });
    expect(toUserMessage(error)).toBe('Já existe um registro com os dados informados.');
  });

  it('describes Inventory unavailability on 503', () => {
    const error = new HttpErrorResponse({ status: 503 });
    expect(toUserMessage(error)).toContain('temporariamente indisponível');
  });

  it('falls back to a generic message for unexpected statuses', () => {
    const error = new HttpErrorResponse({ status: 500 });
    expect(toUserMessage(error)).toContain('erro inesperado');
  });
});
