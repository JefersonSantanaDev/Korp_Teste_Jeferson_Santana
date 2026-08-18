# Korp Web

SPA Angular 21 do desafio Korp — cadastro de produtos e emissão/fechamento de notas fiscais. Ver o `README.md` na raiz do repositório para a visão geral do projeto (arquitetura, como subir tudo via Docker) e `DESIGN.md` neste diretório para as decisões de UI/UX.

## Pré-requisitos

Node.js compatível com Angular 21. As APIs Inventory (`:5001`) e Billing (`:5002`) precisam estar no ar — via `docker compose up` na raiz do repositório, ou `dotnet run` localmente (ver README raiz).

## Comandos

Instalar dependências:

```bash
npm ci
```

Servidor de desenvolvimento (`http://localhost:4200`, recarrega ao salvar):

```bash
npm start
```

Build de produção:

```bash
npm run build
```

Testes (Vitest):

```bash
npm test -- --watch=false
```

## Estrutura

```text
src/app/
├── app.config.ts / app.routes.ts   Providers e rotas raiz
├── app.ts / app.html / app.scss    Shell (topbar + router-outlet)
├── features/
│   ├── products/                   Listagem e cadastro de produto
│   │   ├── data-access/            ProductsService (HTTP)
│   │   ├── models/
│   │   └── pages/
│   └── invoices/                   Listagem, criação e detalhe/fechamento de nota
│       ├── data-access/            InvoicesService (HTTP)
│       ├── models/
│       └── pages/
└── shared/
    ├── models/                     ProblemDetails
    └── utils/                      Tradução de erro de API, formatação do número da nota
src/environments/                    URLs base das APIs (Inventory/Billing)
src/styles/                          Tokens de design + classes utilitárias compartilhadas
```

Rotas lazy-loaded por feature: `/produtos`, `/produtos/novo`, `/notas`, `/notas/nova`, `/notas/:id`.

## Decisões relevantes

- **Signals** para estado local de página (dados, loading, erro, processamento); **RxJS** só para compor as chamadas HTTP (`catchError`, `finalize`).
- **Reactive Forms** estáveis — não os Signal Forms experimentais.
- Formulários de produto e de nova nota são **páginas de rota**, não modais — evita construir um dialog acessível do zero sem biblioteca de UI.
- Erros de API nunca aparecem crus: `shared/utils/api-error.util.ts` traduz qualquer resposta de erro para uma mensagem fixa em português por status HTTP.
- Impressão da nota reaproveita a própria tela de detalhe (CSS `@media print` + `.no-print`), sem view dedicada nem geração de PDF.

Detalhamento completo de identidade visual, componentes e estados de UI em `DESIGN.md`.
