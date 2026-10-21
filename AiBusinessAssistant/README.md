# AI Business Assistant API

ASP.NET Core 8 Web API for customer, product, and order management, dashboard summaries, and the business assistant chat workflow. The API uses SQLite for persistence and Microsoft Semantic Kernel with a locally hosted Ollama model for chat.

## Requirements

- .NET 8 SDK
- SQLite (the app uses the EF Core SQLite provider; a separate SQLite server is not required)
- Ollama with the configured model available for AI chat features

## Configuration

Settings are read from `appsettings.json` and may be overridden by environment-specific settings or environment variables.

| Setting | Purpose | Default |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | SQLite database connection string | `Data Source=businessai.db` |
| `Ollama:Endpoint` | Ollama server URL | `http://localhost:11434` |
| `Ollama:ModelId` | Ollama model name | `phi3:mini` |
| `Ollama:MaxHistoryMessages` | Maximum conversation messages sent as history | `20` |
| `Ollama:MaxMessageLength` | Maximum accepted chat message length | `4000` |
| `Ollama:RequestTimeoutSeconds` | Chat request timeout | `90` |

For example, configure Ollama using environment variables:

```powershell
$env:Ollama__Endpoint = "http://localhost:11434"
$env:Ollama__ModelId = "phi3:mini"
```

Start Ollama and pull the configured model before using chat:

```bash
ollama pull phi3:mini
```

The Ollama values above can be changed in `appsettings.json` or overridden per environment. The API validates the endpoint and model configuration at startup.

## Database setup

The database file path is relative to the API working directory by default. Existing migrations are in `Migrations/`. If setting up a fresh database, apply them with the EF Core command-line tool installed:

```bash
dotnet ef database update
```

To use another database file, set `ConnectionStrings__DefaultConnection`, for example `Data Source=./data/businessai.db` (ensure the directory exists).

## Run locally

From this directory:

```bash
dotnet restore
dotnet run --launch-profile https
```

The HTTPS profile listens on `https://localhost:7152` and `http://localhost:5252`. In Development, Swagger UI is available at [https://localhost:7152/swagger](https://localhost:7152/swagger).

## Build

```bash
dotnet build
```

## Main endpoints

All routes are under `/api`.

| Method | Route | Purpose |
| --- | --- | --- |
| `GET`, `POST` | `/customers` | List or create customers |
| `GET`, `PUT`, `DELETE` | `/customers/{id}` | Read, update, or delete a customer |
| `GET`, `POST` | `/products` | List or create products |
| `GET`, `PUT`, `DELETE` | `/products/{id}` | Read, update, or delete a product |
| `GET` | `/products/sku/{sku}` | Find a product by SKU |
| `GET`, `POST` | `/orders` | List or create orders |
| `GET`, `DELETE` | `/orders/{id}` | Read or delete an order |
| `POST` | `/orders/{id}/complete` | Complete a pending order |
| `GET` | `/dashboard/summary` | Get dashboard metrics and recent records |
| `POST` | `/chat` | Send a message to the business assistant |
| `GET`, `POST` | `/chat/{conversationId}/order-workflow` | Read or advance an assistant order workflow |

Order prices and totals are computed by the API using current product prices. Inventory is reserved when an order is created; an order is rejected if the requested quantity exceeds available stock.
