# E-Commerce Back-End API

A clean and scalable **E-Commerce Back-End API** built with **ASP.NET Core** and **Clean Architecture**.

## Project Overview

This project provides the back-end services for an e-commerce system, including authentication, products, categories, carts, orders, payments, shipments, reviews, and user management.

## Technologies Used

* ASP.NET Core
* Entity Framework Core
* SQL Server
* Clean Architecture
* Repository Pattern
* Unit of Work Pattern
* JWT Authentication
* FluentValidation
* Swagger / OpenAPI
* Rate Limiting
* Logging

## Project Structure

```text
ECommerce.Api/
├── Application/
├── Contracts/
├── Domain/
├── ECommerce.Api/
├── Infrastructure/
└── ECommerce.sln
```

## Main Features

* User registration and login
* JWT authentication and refresh tokens
* Role-based authorization
* Product management
* Category management
* Product images
* Shopping cart
* Order management
* Payment management
* Shipment lifecycle
* Product reviews
* Validation using FluentValidation
* Global exception handling
* Rate limiting
* Logging

## Architecture

The project follows **Clean Architecture** principles:

* **Domain**: Contains core entities and enums.
* **Application**: Contains DTOs, interfaces, services, validators, and business logic.
* **Infrastructure**: Contains database context, repositories, identity services, and external implementations.
* **Contracts**: Contains API request and response models.
* **ECommerce.Api**: Contains controllers, middlewares, and API configuration.

## Authentication

The API uses **JWT Bearer Authentication** with roles such as:

* Customer
* Admin
* SuperAdmin

## API Documentation

Swagger is used to test and document the API endpoints.

After running the project, open:

```text
https://localhost:{port}/swagger
```

## Database

The project uses **SQL Server** with Entity Framework Core.

## How to Run

1. Clone the repository:

```bash
git clone https://github.com/your-username/E-Commerce---back-end-Api.git
```

2. Open the solution:

```text
ECommerce.sln
```

3. Update the connection string in `appsettings.json`.

4. Run the database migrations or use the existing database configuration.

5. Run the project from Visual Studio or using:

```bash
dotnet run
```

## Author

Omar Alkaseh

## License

This project is for learning and development purposes.
