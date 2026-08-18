# Korp — Desafio Técnico

Sistema de emissão de notas fiscais com controle de estoque: cadastro de produtos, criação de notas com múltiplos itens, e fechamento com baixa de estoque atômica, idempotente e protegida contra concorrência. Dois microsserviços .NET 10 independentes (Inventory e Billing), cada um com seu próprio banco PostgreSQL, e uma SPA Angular 21.

## Sobre

O ponto mais relevante tecnicamente não é o CRUD de produtos ou notas — é o **fechamento da nota**, que envolve dois serviços independentes coordenados via HTTP síncrono, sem transação distribuída: consistência garantida por idempotência, retry seguro e uma atualização condicional no Postgres para proteção de concorrência. Ver `docs/architecture.md` e `docs/technical-details.md` para o detalhamento.

## Arquitetura

```text
Angular 21 SPA
    ├── REST → Inventory.Api (.NET 10) → inventory_db
    └── REST → Billing.Api (.NET 10)   → billing_db
                         └── REST → Inventory.Api

PostgreSQL 17 · Docker Compose · EF Core + Npgsql
```

Inventory é dono exclusivo do domínio de estoque; Billing é dono exclusivo do domínio de notas. Nenhum dos dois acessa o banco do outro — o único contrato entre eles é HTTP. Ver `docs/architecture.md` para o diagrama completo e os fluxos de criação/fechamento de nota.

## Tecnologias

| Área | Tecnologia |
| --- | --- |
| Frontend | Angular 21, TypeScript strict, SCSS, Signals + RxJS, Reactive Forms |
| Testes frontend | Vitest |
| Backend | .NET 10, ASP.NET Core Controllers, EF Core + Npgsql |
| Testes backend | xUnit + Testcontainers (PostgreSQL real, não InMemory) |
| Banco | PostgreSQL 17 — `inventory_db` e `billing_db` |
| Documentação de API | OpenAPI nativo + Swagger UI, em cada serviço |
| Orquestração local | Docker Compose |

## Estrutura do repositório

```text
frontend/korp-web/                          Angular SPA
services/inventory/src/Inventory.Api/       Inventory — API .NET 10
services/inventory/tests/Inventory.Tests/   Inventory — testes xUnit
services/billing/src/Billing.Api/           Billing — API .NET 10
services/billing/tests/Billing.Tests/       Billing — testes xUnit
infra/postgres/init/                        Script de criação dos bancos
docs/                                        architecture.md, technical-details.md
Korp.sln                                     Solution .NET
docker-compose.yml                           Orquestração local
```

## Pré-requisitos

- Docker e Docker Compose (caminho recomendado — sobe tudo sem instalar mais nada)
- Para desenvolvimento local sem Docker nas APIs/frontend: .NET SDK 10, Node.js compatível com Angular 21, PostgreSQL (ou o container do Compose)

## Como executar

### Opção recomendada — tudo via Docker

```powershell
git clone <url-do-repositorio>
cd Korp_Teste_Jeferson_Santana
docker compose up --build
```

Sobe os quatro containers (Postgres, Inventory, Billing, Angular servido por Nginx). **Migrations são aplicadas automaticamente no startup de cada API** — não é preciso rodar nenhum comando de migration manualmente, mesmo em um clone limpo com o banco vazio.

Para derrubar tudo mantendo os dados:

```powershell
docker compose down
```

Para derrubar tudo e apagar o banco (o próximo `up` recria do zero):

```powershell
docker compose down -v
```

### Alternativa — desenvolvimento local com hot reload

Suba só o Postgres via Docker e rode as três aplicações localmente, cada uma em um terminal:

```powershell
docker compose up -d postgres
```

```powershell
dotnet run --project services/inventory/src/Inventory.Api --launch-profile http
```

```powershell
dotnet run --project services/billing/src/Billing.Api --launch-profile http
```

```powershell
cd frontend/korp-web
npm ci
npm start
```

Não rode as duas opções ao mesmo tempo — as portas `5001`, `5002` e `4200` colidem entre o container e o processo local.

## URLs

| Aplicação | URL |
| --- | --- |
| Frontend (Angular) | http://localhost:4200 |
| Inventory — Swagger | http://localhost:5001/swagger |
| Inventory — OpenAPI | http://localhost:5001/openapi/v1.json |
| Billing — Swagger | http://localhost:5002/swagger |
| Billing — OpenAPI | http://localhost:5002/openapi/v1.json |

Arquivos `.http` (`services/inventory/src/Inventory.Api/Inventory.Api.http`, `services/billing/src/Billing.Api/Billing.Api.http`) também permitem testar as APIs direto do editor, sem depender do Swagger.

## Banco de dados

Um único container PostgreSQL 17, dois bancos lógicos (`inventory_db`, `billing_db`), criados automaticamente por `infra/postgres/init/`. Credenciais padrão (`.env.example`) são exclusivamente de desenvolvimento local — nunca usadas em produção real.

## Fluxos principais

1. **Produto**: cadastro com código único, descrição e saldo (nunca negativo).
2. **Nota**: criada como `Open`, com número sequencial gerado pelo Postgres; cada item guarda um snapshot do código/descrição do produto no momento da criação. Criar uma nota **não** baixa estoque.
3. **Fechamento**: aciona a baixa de estoque no Inventory (atômica, idempotente, protegida contra concorrência) e só marca a nota como `Closed` depois de confirmação — nunca antes. O Angular chama `window.print()` só após o fechamento ser confirmado.

Detalhamento completo em `docs/architecture.md`.

## Tratamento de falhas

Se o Inventory estiver indisponível ou não responder a tempo, o Billing retorna `503`, a nota permanece `Open`, e o Angular mostra uma mensagem contextual em português com uma opção segura de tentar novamente — nunca uma tela de erro genérica.

## Idempotência

Toda tentativa de baixa de estoque usa `operationId = invoiceId`. Repetir uma baixa já processada (ex.: depois de um retry causado por perda de resposta) nunca desconta o estoque duas vezes — o Inventory reconhece a operação e responde `already_processed`, que o Billing trata como sucesso. Mecanismo detalhado em `docs/technical-details.md`.

## Concorrência

Duas notas disputando a última unidade de um produto nunca podem as duas ter sucesso: uma atualização condicional no Postgres (`UPDATE ... WHERE stock >= quantidade`) garante exatamente um sucesso e um `409 Conflict`, sem estoque negativo. Provado com testes automatizados disparando requisições HTTP verdadeiramente concorrentes — não é só uma alegação de design.

## Testes

```powershell
dotnet test Korp.sln
```

16 testes `xUnit` (Inventory + Billing), contra PostgreSQL real via Testcontainers — cobrindo as regras de produto, nota, fechamento, falha do Inventory, idempotência e concorrência.

```powershell
cd frontend/korp-web
npm test -- --watch=false
```

30 testes Vitest cobrindo os serviços HTTP, a tradução de erros da API e os fluxos de UI (validação, estados de loading/erro, fechamento).

## Decisões técnicas

- **Monorepo, não monólito**: um repositório simplifica a entrega e a execução do desafio, mas cada aplicação mantém dependências, processo, porta, Dockerfile e persistência próprios.
- **REST síncrono, sem mensageria**: o fechamento precisa de feedback imediato para o usuário; falhas parciais são resolvidas com idempotência e retry seguro, não com filas.
- **Sem transação distribuída**: cada serviço controla sua própria transação local; consistência entre os dois vem de idempotência e convergência eventual.
- **Sem NgRx**: o estado é pequeno o suficiente para Signals + RxJS cobrirem tudo sem a complexidade extra.
- **Sem Clean Architecture multi-projeto**: um projeto executável por serviço, organizado por feature — evita boilerplate desproporcional ao prazo e ao tamanho do domínio.

## Trade-offs e melhorias futuras

- Migrations aplicadas automaticamente no startup simplificam a execução (`docker compose up --build` funciona sem passo manual), mas não seria a escolha para múltiplas réplicas em produção real — lá, um passo de migration explícito e separado do boot da aplicação seria mais seguro.
- Autenticação está fora do escopo deste desafio.
- Impressão é HTML + `window.print()`, sem geração de PDF — suficiente para o requisito, sem a complexidade de uma biblioteca de PDF.

## Documentação relacionada

- `docs/architecture.md` — arquitetura, responsabilidades de cada serviço, fluxos de criação e fechamento de nota.
- `docs/technical-details.md` — detalhamento técnico do Angular, do .NET, idempotência, concorrência, mapeamento de erros e testes.
- `frontend/korp-web/DESIGN.md` — identidade visual, componentes, estados de UI e decisões de UX.
- `frontend/korp-web/README.md` — comandos específicos do frontend.
