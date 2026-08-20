# Sistema de Emissão de Notas Fiscais

Projeto desenvolvido como teste técnico para a Korp. A solução implementa cadastro de produtos, controle de estoque e emissão de notas fiscais com fechamento integrado ao estoque.

## Visão geral

A aplicação é dividida em dois serviços de backend e um frontend Angular:

- **Inventory API**: cadastro de produtos e controle de estoque.
- **Billing API**: criação de notas fiscais, gerenciamento de itens e fechamento da nota.
- **Frontend**: interface para produtos, notas fiscais e fluxo de fechamento.
- **PostgreSQL**: um banco independente para cada API.

```mermaid
flowchart LR
    U[Usuário] --> F[Angular Frontend<br/>localhost:4200]
    F --> I[Inventory API<br/>localhost:5173]
    F --> B[Billing API<br/>localhost:5007]
    B -->|consulta produto / baixa de estoque| I
    I --> IDB[(Inventory DB<br/>PostgreSQL :5432)]
    B --> BDB[(Billing DB<br/>PostgreSQL :5433)]
```

## Tecnologias

### Backend

- C# / .NET 8
- ASP.NET Core Web API
- Entity Framework Core
- PostgreSQL
- Swagger / OpenAPI
- xUnit

### Frontend

- Angular 21
- Angular Material
- RxJS
- Vitest

### Infraestrutura

- Docker
- Docker Compose
- Nginx para servir o frontend no container

## Estrutura do projeto

```text
.
├── backend/
│   ├── Inventory.Api/
│   └── Billing.Api/
├── frontend/
├── tests/
│   ├── Inventory.Api.Tests/
│   └── Billing.Api.Tests/
├── docker-compose.yml
└── Korp_Teste_GuilhermeBaeta.sln
```

## Regras de negócio implementadas

- O código do produto deve ser único.
- O estoque inicial não pode ser negativo.
- Cada nota é criada com status **Open**.
- A numeração segue o formato `NF-AAAA-000001`, com sequência anual.
- Um mesmo produto não pode ser adicionado duas vezes à mesma nota.
- Itens só podem ser adicionados, alterados ou removidos enquanto a nota estiver aberta.
- Uma nota precisa ter pelo menos um item para ser fechada.
- Ao fechar a nota, a Billing API solicita a baixa de estoque à Inventory API.
- A baixa só é concluída quando há estoque suficiente para todos os produtos solicitados.
- A nota só é marcada como **Closed** depois que a Inventory API confirma a baixa de estoque.
- O fechamento é definitivo no fluxo atual da aplicação.

## Executando com Docker

Esta é a forma mais simples de executar a solução completa.

### Pré-requisito

- Docker Desktop ou ambiente com Docker Compose disponível.

Na raiz do projeto:

```bash
docker compose up --build -d
```

Verifique os containers:

```bash
docker compose ps
```

A aplicação ficará disponível em:

| Serviço | URL |
| --- | --- |
| Frontend | http://localhost:4200 |
| Inventory API | http://localhost:5173 |
| Inventory Health | http://localhost:5173/health |
| Inventory Swagger | http://localhost:5173/swagger |
| Billing API | http://localhost:5007 |
| Billing Health | http://localhost:5007/health |
| Billing Swagger | http://localhost:5007/swagger |
| Inventory PostgreSQL | localhost:5432 |
| Billing PostgreSQL | localhost:5433 |

As migrations do Entity Framework são aplicadas automaticamente quando cada API inicia.

Para acompanhar os logs:

```bash
docker compose logs -f inventory-api billing-api frontend
```

Para encerrar os containers:

```bash
docker compose down
```

Para remover também os volumes e recriar os bancos do zero:

```bash
docker compose down -v
```

## Executando localmente

### Pré-requisitos

- .NET SDK 8.0.422 ou patch compatível com o `global.json`.
- PostgreSQL, ou Docker para subir apenas os bancos.
- Node.js e npm compatíveis com Angular 21.

### 1. Subir os bancos

Na raiz do projeto:

```bash
docker compose up -d inventory-db billing-db
```

### 2. Inventory API

Em um terminal:

```bash
dotnet run --project backend/Inventory.Api/Inventory.Api.csproj
```

Disponível em `http://localhost:5173`.

### 3. Billing API

Em outro terminal:

```bash
dotnet run --project backend/Billing.Api/Billing.Api.csproj
```

Disponível em `http://localhost:5007`.

### 4. Frontend

Em outro terminal:

```bash
cd frontend
npm install
npm start
```

Disponível em `http://localhost:4200`.

## Build e testes

### Backend

Na raiz do projeto:

```bash
dotnet restore
dotnet build Korp_Teste_GuilhermeBaeta.sln
dotnet test Korp_Teste_GuilhermeBaeta.sln
```

Os testes de backend cobrem regras dos serviços de produtos, itens da nota e fechamento da nota, incluindo a integração HTTP simulada entre Billing e Inventory.

### Frontend

Na pasta `frontend`:

```bash
npm install
npm run build
npm test -- --watch=false
```

Os testes de frontend validam as chamadas HTTP dos serviços responsáveis por produtos e notas fiscais.

## Health checks

As duas APIs expõem um endpoint simples de saúde para facilitar diagnóstico local, monitoramento e validações de infraestrutura:

```text
GET http://localhost:5173/health
GET http://localhost:5007/health
```

Quando a aplicação está disponível, o endpoint responde com status HTTP `200` e o estado `Healthy`.

## Principais endpoints

### Inventory API

| Método | Endpoint | Descrição |
| --- | --- | --- |
| `POST` | `/api/products` | Cadastra um produto |
| `GET` | `/api/products` | Lista os produtos |
| `GET` | `/api/products/{id}` | Consulta um produto por ID |
| `POST` | `/api/products/deduct-stock` | Realiza baixa de estoque |

### Billing API

| Método | Endpoint | Descrição |
| --- | --- | --- |
| `POST` | `/api/invoices` | Cria uma nota fiscal |
| `GET` | `/api/invoices` | Lista as notas fiscais |
| `GET` | `/api/invoices/{id}` | Consulta uma nota por ID |
| `POST` | `/api/invoices/{id}/close` | Fecha a nota e solicita a baixa de estoque |
| `GET` | `/api/invoices/{invoiceId}/items` | Lista os itens da nota |
| `POST` | `/api/invoices/{invoiceId}/items` | Adiciona um item |
| `PUT` | `/api/invoices/{invoiceId}/items/{itemId}` | Altera a quantidade de um item |
| `DELETE` | `/api/invoices/{invoiceId}/items/{itemId}` | Remove um item |

## Tratamento de erros

As APIs utilizam respostas padronizadas para erros de validação e regras de negócio. Quando aplicável, a resposta contém:

```json
{
  "statusCode": 400,
  "message": "Descrição do erro.",
  "traceId": "identificador-da-requisicao"
}
```

Entre os cenários tratados estão produto inexistente, código duplicado, estoque insuficiente, nota inexistente, alteração de nota já fechada e indisponibilidade da Inventory API.

## Fluxo principal

1. Cadastrar produtos e seus estoques.
2. Criar uma nova nota fiscal.
3. Adicionar produtos à nota e definir as quantidades.
4. Alterar ou remover itens enquanto a nota estiver aberta.
5. Fechar a nota.
6. A Billing API solicita a baixa de estoque à Inventory API.
7. Com a baixa confirmada, a nota passa para o status **Closed**.

## Observações de arquitetura

Cada serviço possui seu próprio banco de dados, mantendo separadas as responsabilidades de estoque e faturamento. A comunicação entre Billing e Inventory é HTTP síncrona e encapsulada no `InventoryApiClient`.

A solução prioriza simplicidade e clareza para o escopo do teste técnico. Em um cenário de produção com maior volume e requisitos de resiliência, a integração entre serviços poderia evoluir para mecanismos de mensageria, idempotência e consistência eventual.
