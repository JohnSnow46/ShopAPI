# ShopAPI

![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet&logoColor=white)
![EF Core](https://img.shields.io/badge/EF_Core-8.0-512BD4)
![SQLite](https://img.shields.io/badge/SQLite-dev-003B57?logo=sqlite&logoColor=white)
![JWT](https://img.shields.io/badge/Auth-JWT_Bearer-000000?logo=jsonwebtokens&logoColor=white)
![SignalR](https://img.shields.io/badge/SignalR-real--time-0078D7)
![xUnit](https://img.shields.io/badge/Tests-xUnit-green)

REST API for an e-commerce shop built with ASP.NET Core 8 following Clean Architecture principles. Covers the full shopping flow: product catalog, cart management, order processing with status tracking, and real-time push notifications via SignalR. Intended as a portfolio project demonstrating layered architecture, the Result pattern, and JWT auth.

## Features

- 🛍️ Product catalog with filtering by category, full-text search, and pagination
- 🛒 Per-user shopping cart (add, remove, clear)
- 📦 Order lifecycle — create from cart, track status (Pending → Confirmed → Shipped → Delivered / Cancelled)
- 🔔 Real-time SignalR notifications: new orders, status changes, low-stock alerts
- 🔐 JWT authentication with role-based authorization (User / Admin)
- 📄 OpenAPI / Swagger UI with XML doc comments

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 8 |
| ORM | Entity Framework Core 8 |
| Database | SQLite (dev) |
| Auth | JWT Bearer |
| Real-time | SignalR |
| Testing | xUnit + Moq + `Microsoft.AspNetCore.Mvc.Testing` |

## Architecture

Clean Architecture split into 5 projects:

```
┌────────────────────────────────────────────────────┐
│                    ShopAPI.API                     │
│        Controllers · OrderHub · Middleware         │
└──────────────────────┬─────────────────────────────┘
                       │
         ┌─────────────▼──────────────┐
         │     ShopAPI.Application    │
         │  Use Cases · DTOs · Result │
         └──────┬───────────────┬─────┘
                │               │
   ┌────────────▼─────┐  ┌──────▼──────────────────┐
   │  ShopAPI.Domain  │◄─┤ ShopAPI.Infrastructure   │
   │ Entities · Repos │  │ EF Core · Repositories   │
   └──────────────────┘  └──────────────────────────┘
```

**Dependency flow:** `API → Application → Domain ← Infrastructure`

Key patterns:
- Each use case = one class under `Application/UseCases/<Domain>/` with a single `ExecuteAsync(...)` method.
- All use cases return `Result<T>` — never throw to controllers.
- Controllers map `Result<T>` to HTTP: `!IsSuccess` → `BadRequest`/`NotFound`, success → `Ok`/`CreatedAtAction`.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- EF Core CLI: `dotnet tool install --global dotnet-ef`

### Configuration

Open `ShopAPI.API/appsettings.json` and replace the JWT key with a strong secret (minimum 32 characters):

```json
"Jwt": {
  "Key": "YOUR_SECRET_KEY_MIN_32_CHARS_REPLACE_ME"
}
```

For local development, create `ShopAPI.API/appsettings.Development.json` (git-ignored) to override any settings without touching the committed file.

### Run

```bash
dotnet run --project ShopAPI.API
```

- Swagger UI: `https://localhost:7xxx/swagger`
- The SQLite database (`shop.db`) is created and migrated automatically on first run.

### Seed data

`DataSeeder` seeds on every startup (idempotent):

| Role | Email | Password |
|------|-------|----------|
| Admin | `admin@shop.com` | `Admin123!` |

3 example products across Electronics, Clothing, and Food categories.

## API Endpoints

### Auth — `/api/auth`

| Method | Endpoint | Auth | Description |
|--------|----------|:----:|-------------|
| POST | `/api/auth/register` | — | Register a new user account |
| POST | `/api/auth/login` | — | Authenticate and receive a JWT token |

### Products — `/api/products`

| Method | Endpoint | Auth | Description |
|--------|----------|:----:|-------------|
| GET | `/api/products` | — | List products (`?category=`, `?search=`, `?page=`, `?pageSize=`) |
| GET | `/api/products/{id}` | — | Get a single product |
| POST | `/api/products` | Admin | Create a product |
| PUT | `/api/products/{id}` | Admin | Update a product |
| DELETE | `/api/products/{id}` | Admin | Delete a product |

### Cart — `/api/cart`

| Method | Endpoint | Auth | Description |
|--------|----------|:----:|-------------|
| GET | `/api/cart` | User | View the current cart |
| POST | `/api/cart/items` | User | Add a product (or increment quantity) |
| DELETE | `/api/cart/items/{productId}` | User | Remove a product from the cart |
| DELETE | `/api/cart` | User | Clear the entire cart |

### Orders — `/api/orders`

| Method | Endpoint | Auth | Description |
|--------|----------|:----:|-------------|
| POST | `/api/orders` | User | Create an order from the cart (clears cart) |
| GET | `/api/orders` | User/Admin | List orders (users see own; admins see all) |
| GET | `/api/orders/{id}` | User/Admin | Get order details |
| PUT | `/api/orders/{id}/status` | Admin | Update order status |

## Business Rules

### Order status flow

```
┌─────────┐     ┌───────────┐     ┌──────────┐     ┌───────────┐
│ Pending │────►│ Confirmed │────►│ Shipped  │────►│ Delivered │
└────┬────┘     └─────┬─────┘     └────┬─────┘     └───────────┘
     │                │                │
     └────────────────┴────────────────┘
                      │
                 ┌────▼──────┐
                 │ Cancelled │
                 └───────────┘
```

- An order is created from cart contents; the cart is cleared on success.
- Low-stock alerts (`StockLow` SignalR event) are sent to admins when stock drops below threshold after order creation.

### SignalR — `/hubs/orders`

Pass the JWT token as a query parameter for WebSocket connections: `?access_token=<token>`

On connect, users join group `user-{userId}`; admins also join `admins`.

| Event | Recipients | Trigger |
|-------|-----------|---------|
| `NewOrderPlaced(order)` | admins | New order created |
| `OrderStatusChanged(order)` | user + admins (terminal states) | Status updated |
| `StockLow(productId, name, qty)` | admins | Stock drops below threshold on order |

## Testing

```bash
# Run all tests
dotnet test

# Run a single test class
dotnet test --filter "FullyQualifiedName~ClassName"

# Run a single test method
dotnet test --filter "FullyQualifiedName~ClassName.MethodName"
```
