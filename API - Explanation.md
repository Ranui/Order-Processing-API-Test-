# Order Processing API - Explanation

## Overview & How to Run the API
This is a minimal REST API built with **ASP.NET Core Minimal APIs** that exposes product data from a database. It uses **Entity Framework Core** (`AppDbContext`) for data access, **SQLite** for it's lightweight database and a **DTO (Data Transfer Object)** pattern to control what data is returned to the client.

##### Prerequisites:
>- .NET 8 SDK
>- No database server required, SQLite runs as a local file.

##### Steps
>- Clone / download the project and navigate to the root folder.
>- Set the connection string in appsettings.json (or appsettings.Development.json):
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=TestDB.db"
  }
}
```
>- Run the API using the command: dotnet run
>- Open Swagger UI to explore and test the endpoints interactively: https://localhost:{port}/swagger

##### Project Structure
```
Order_Processing_API/
├── Endpoints/
│   ├── ProductEndpoints.cs     # GET /products, GET /products/{id}, PUT /products/{id}
│   └── OrderEndpoints.cs       # POST /orders, GET /orders/{id}, POST /orders/{id}/confirm, POST /orders/{id}/cancel
├── DTOs/
│   ├── ProductDto.cs           # Public shape for product data
│   ├── OrderDto.cs             # Public shape for order data
│   ├── OrderItemDto.cs         # Public shape for individual order line items
│   └── CreateOrderDto.cs       # Input shape for creating a new order
├── Models/                     # EF Core entity classes (scaffolded from DB)
├── TestDB.db                   # SQLite database
└── Program.cs                  # App entry point, DI registration, middleware
```

## API Reference

### Products

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/products` | List all products |
| `GET` | `/products/{id}` | Get a single product |
| `PUT` | `/products/{id}` | Update a product's name, price, or stock |

### Orders

| Method | Route | Description |
|--------|-------|-------------|
| `POST` | `/orders` | Create a new draft order |
| `GET` | `/orders/{id}` | Get an order with its line items |
| `POST` | `/orders/{id}/confirm` | Confirm a draft order (deducts stock) |
| `POST` | `/orders/{id}/cancel` | Cancel an order (restores stock if Confirmed) |

## DTO Pattern - Why it matters
> By using the **DTO (Data Transfer Object)** we can carry data between processes without exposing sensitive parts of our API or private user information. 

It acts as a mask that helps us display only the data the user needs to see instead of straight up displaying the whole database table(s).

## Database Structure (SQLite)
> I chose SQLite in order to avoid all the intricacies of in memory DBs and server based DBs. We needed something lightweight and easily testable so SQLite was a nobrainer. 

### Products

| Column | Type | Notes |
|--------|------|-------|
| `Id` | TEXT | Primary key (e.g. `"P1001"`) |
| `Name` | TEXT | Product name |
| `UnitPrice` | REAL | Price per unit |
| `AvailableStock` | INTEGER | Current available stock (default `0`) |

### Orders

| Column | Type | Notes |
|--------|------|-------|
| `Id` | INTEGER | Auto-increment primary key |
| `CustomerEmail` | TEXT | Required |
| `Status` | TEXT | `"Draft"`, `"Confirmed"`, or `"Cancelled"` (default `"Draft"`) |
| `CreatedAtUtc` | TEXT | Stored as formatted string (`yyyy-MM-dd HH:mm:ssZ`) |
| `ConfirmedAtUtc` | TEXT | Nullable |
| `CancelledAtUtc` | TEXT | Nullable |
| `TotalAmount` | REAL | Sum of all line totals (default `0.0`) |

### OrderItems

| Column | Type | Notes |
|--------|------|-------|
| `OrderId` | INTEGER | Foreign key → Orders |
| `ProductId` | TEXT | Foreign key → Products |
| `ProductName` | TEXT | Snapshot of name at time of order |
| `UnitPrice` | REAL | Snapshot of price at time of order |
| `Quantity` | INTEGER | Units ordered |
| `LineTotal` | REAL | `UnitPrice × Quantity` |

## What I Would Improve Next (Given more time, the priorities would be in the order that follows)
1. **Tests**: Unit and integration tests for the core business rules
2. **Query Optimization**: Add limitations and checks, returning everything from a table/DB on every response wouldn't scale well with large databases.
3. **Error response shape**: Standardize all error responses to a consistent JSON body rather than plain strings.

# Project time:
| Task | Time                        |
|------|-----------------------------|
| Database creation + Entity Framework scaffolding | Aproximately 1 hr           |
| Base API creation (endpoints, DTOs, business rules) | Aproximately 1hr 50min      |
| README | Aproximately 15mins         |
| **Total** | **Aproximately 3 hr 5 min** |
