# Korp — Desafio Técnico

## Executar as APIs e acessar o Swagger

Partindo da raiz do repositório:

```text
PS C:\Projects\Korp_Teste_Jeferson_Santana>
```

Abra dois terminais. Cada API precisa continuar em execução no próprio terminal enquanto o Swagger estiver sendo utilizado.

### Terminal 1 — Inventory

Execute:

```powershell
dotnet run --project services/inventory/src/Inventory.Api --launch-profile http
```

Quando o terminal mostrar uma mensagem semelhante a esta, a API estará pronta:

```text
Now listening on: http://localhost:5001
```

Acesse o Swagger do Inventory:

```text
http://localhost:5001/swagger
```

### Terminal 2 — Billing

Execute:

```powershell
dotnet run --project services/billing/src/Billing.Api --launch-profile http
```

Quando o terminal mostrar uma mensagem semelhante a esta, a API estará pronta:

```text
Now listening on: http://localhost:5002
```

Acesse o Swagger do Billing:

```text
http://localhost:5002/swagger
```

> Neste estágio do projeto, o Swagger pode mostrar `No operations defined in spec!`. Isso é esperado porque os endpoints de exemplo `WeatherForecast` foram removidos e os endpoints reais serão implementados nas próximas etapas.

Para encerrar uma API, volte ao terminal em que ela está rodando e pressione `Ctrl+C`.
