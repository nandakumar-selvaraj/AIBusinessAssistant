# AI Business Assistant

An AI-powered business management application built with **ASP.NET Core 8, Angular 22, SQLite, Microsoft Semantic Kernel, and Ollama**.

The application combines a business dashboard and CRUD operations with a conversational AI assistant that helps users interact with customer, product, and order data through a chat interface.

The project currently uses a locally hosted Ollama model (`phi3:mini`) for AI functionality, avoiding dependency on a paid, hosted LLM API during local development.

## Features

### Business Management
- **Dashboard:** View business summary information and recent records.
- **Customer Management:** Create, view, update, and delete customer records.
- **Product Management:** Manage product information, pricing, SKU, and available inventory.
- **Order Management:** Create, view, and manage customer orders.
- **Order Completion:** Complete pending orders through the API and assistant workflow.

### AI Business Assistant
- Interact with business data through a conversational chat interface.
- Use Microsoft Semantic Kernel to integrate the language model with application functionality.
- Select customers and products through the assistant's interactive workflow.
- Specify product quantities and review order details before confirming.
- Confirm or cancel order creation.
- List pending orders, inspect order items, and select an order for completion.
- Confirm or cancel order completion.
- Maintain conversation context using conversation identifiers and chat history.

**Important:** Business operations such as inventory validation, pricing, and order creation must be validated by the backend rather than relying solely on AI-generated responses.

## Technology Stack

| Component | Technology |
|---|---|
| Frontend | Angular 22 |
| UI styling | Bootstrap 5 |
| Backend | ASP.NET Core 8 Web API |
| Database | SQLite |
| ORM | Entity Framework Core |
| AI orchestration | Microsoft Semantic Kernel |
| Local language model runtime | Ollama |
| Configured model | `phi3:mini` |

## Repository Structure

```text
AIBusinessAssistant/
├── AiBusinessAssistant/
│   ├── Controllers/
│   ├── Features/
│   ├── Extensions/
│   ├── Migrations/
│   ├── appsettings.json
│   ├── AiBusinessAssistant.csproj
│   └── README.md
│
├── ai-business-assistant-ui/
│   ├── src/
│   ├── package.json
│   ├── proxy.conf.json
│   └── README.md
│
└── README.md
```

The root README describes the complete solution. Each application has its own README with component-specific instructions.

- [Backend API README](AiBusinessAssistant/README.md)
- [Angular UI README](ai-business-assistant-ui/README.md)

## Prerequisites

Install the following before running the application:

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js and npm](https://nodejs.org/)
- [Ollama](https://ollama.com/)
- Git

SQLite is used through Entity Framework Core, so a separate SQLite database server is not required.

## Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/nandakumar-selvaraj/AIBusinessAssistant.git
cd AIBusinessAssistant
```

### 2. Start Ollama

Install Ollama and download the configured model:

```bash
ollama pull phi3:mini
```

Ensure the Ollama service is running and the model is available before using the AI assistant. Ollama normally exposes its local API at:

```text
http://localhost:11434
```

You can verify that the model is available with:

```bash
ollama list
```

### 3. Configure and run the backend API

Open a terminal in the repository root:

```bash
cd AiBusinessAssistant
dotnet restore
```

Apply the existing Entity Framework Core migrations when setting up a fresh database. If the EF Core CLI is not installed, install the appropriate .NET 8-compatible `dotnet-ef` tool first.

```bash
dotnet ef database update
```

Start the API:

```bash
dotnet run --launch-profile https
```

The configured development endpoints are:

- HTTPS: `https://localhost:7152`
- HTTP: `http://localhost:5252`
- Swagger UI: `https://localhost:7152/swagger`

Swagger is available in the Development environment.

The default database connection is configured in `AiBusinessAssistant/appsettings.json`:

```text
Data Source=businessai.db
```

The database file is created relative to the API working directory when the application initializes it.

### 4. Run the Angular frontend

Open a second terminal from the repository root:

```bash
cd ai-business-assistant-ui
npm install
npm start
```

Open the application in your browser:

[http://localhost:4200](http://localhost:4200)

The Angular development server forwards `/api` requests to `https://localhost:7152` using `proxy.conf.json`. Ensure the backend is running at the configured address. If you use a different API port or protocol, update the proxy configuration accordingly.

### 5. Try the application

Once both applications are running:

1. Open the dashboard.
2. Create or manage customer records.
3. Add products and configure their prices and stock.
4. Create and manage orders.
5. Open the AI Assistant.
6. Try placing an order by selecting a customer, choosing products, entering quantities, and confirming.
7. Try listing pending orders and completing one through the confirmation workflow.

The AI Assistant requires the Ollama service and configured model to be available.

## Configuration

The API's main settings are in `AiBusinessAssistant/appsettings.json`.

| Setting | Default |
|---|---|
| Database | `Data Source=businessai.db` |
| Ollama endpoint | `http://localhost:11434` |
| Ollama model | `phi3:mini` |
| Maximum history messages | `20` |
| Maximum chat message length | `4000` characters |
| AI request timeout | `90` seconds |

For example, in PowerShell, you can override the Ollama settings for the current terminal session:

```powershell
$env:Ollama__Endpoint = "http://localhost:11434"
$env:Ollama__ModelId = "phi3:mini"
```

Restart the API after changing environment variables. Do not commit secrets, private configuration, or production credentials to the repository.

## Main API Areas

The API exposes endpoints for the following operations:

| Area | Purpose |
|---|---|
| Customers | Customer CRUD operations |
| Products | Product CRUD operations and SKU lookup |
| Orders | Order management and completion |
| Dashboard | Business summary and recent records |
| Chat | Conversational AI interaction |
| Order workflow | Read and advance interactive assistant workflows |

All API routes are under the `/api` prefix. For the exact endpoint methods and routes, see the [API README](AiBusinessAssistant/README.md) or Swagger UI.

## Build and Test

### Backend

From `AiBusinessAssistant/`:

```bash
dotnet build
```

### Frontend

From `ai-business-assistant-ui/`:

```bash
npm run build
```

Run the frontend unit tests with:

```bash
npm test -- --watch=false
```

The frontend test runner uses Vitest with jsdom. Run the commands above in your development environment to verify the current build and test status.

## Architecture Overview

```text
Angular 22 UI
     |
     | HTTP requests (/api)
     v
ASP.NET Core 8 Web API
     |
     +---- Entity Framework Core ---- SQLite
     |
     +---- Semantic Kernel
                  |
                  v
             Ollama Runtime
                  |
                  v
              phi3:mini
```

The Angular application provides the user interface. The ASP.NET Core API handles business operations and persistence. Semantic Kernel connects the AI assistant to application functionality, while Ollama runs the language model locally.

## Current Scope

This is a demonstration project exploring AI-assisted business workflows, local LLM integration, and full-stack application development.

The application is designed for local development and demonstration. Before production deployment, review authentication and authorization, input validation, logging, error handling, database backup and recovery, secrets management, and safeguards for AI-triggered business operations.

## Future Improvements

Potential next steps include:

- Expand automated backend and frontend test coverage.
- Improve AI tool selection and response reliability.
- Strengthen conversation isolation and workflow recovery.
- Add structured logging and improved error handling.
- Evaluate alternative local models if the configured model struggles with tool-driven workflows.
- Prepare deployment configuration and production security controls.

## License

No license has been specified yet. Add a `LICENSE` file if you intend to distribute or reuse this project under an explicit open-source license.
