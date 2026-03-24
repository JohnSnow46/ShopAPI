# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview
ShopAPI — REST API dla sklepu e-commerce: produkty, zamówienia, koszyk, powiadomienia real-time przez SignalR. Portfolio project.

## Commands

```bash
dotnet build
dotnet test
dotnet run --project ShopAPI.API

# Run a single test class or method
dotnet test --filter "FullyQualifiedName~ClassName"
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"

# EF Core migrations
dotnet ef migrations add <Name> --project ShopAPI.Infrastructure --startup-project ShopAPI.API
dotnet ef database update --project ShopAPI.Infrastructure --startup-project ShopAPI.API
```

## Architecture

Clean Architecture — 5 projektów w solution:

| Projekt | Odpowiedzialność |
|---------|-----------------|
| `ShopAPI.Domain` | Encje, enums, interfejsy repozytoriów (IRepository<T>) |
| `ShopAPI.Application` | Use cases (jeden per klasa), DTOs, interfejsy serwisów |
| `ShopAPI.Infrastructure` | EF Core DbContext, implementacje repozytoriów, serwisy zewnętrzne |
| `ShopAPI.API` | Kontrolery, SignalR Hub, middleware (error handling, auth) |
| `ShopAPI.Tests` | xUnit + Moq, testy unit i integracyjne |

**Dependency flow:** API → Application → Domain ← Infrastructure

## Conventions

- **Result\<T\> pattern** zamiast wyjątków w logice biznesowej (Application layer nie rzuca wyjątków do kontrolerów)
- Każdy use case = osobna klasa w `ShopAPI.Application` (np. `CreateOrderUseCase`, `GetProductsUseCase`)
- Async/await dla każdej operacji I/O
- Repozytoria zdefiniowane jako interfejsy w Domain, implementowane w Infrastructure
- Testy dla każdego use case i każdego kontrolera

## Stack

ASP.NET Core 8, EF Core 8, SQLite (dev), xUnit + Moq, SignalR, JWT Bearer

## Additional Docs

- `agent_docs/architecture.md` — szczegóły warstw i zależności
- `agent_docs/business-rules.md` — reguły biznesowe (stany zamówień, stany magazynowe)
- `agent_docs/testing.md` — strategia testowania
