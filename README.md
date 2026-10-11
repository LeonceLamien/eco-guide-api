<div align="center">

# EcoGuide

*A .NET REST API for exploring food products and supporting more sustainable consumption*

![.NET](https://img.shields.io/badge/.NET-8.0-512BD4.svg?logo=dotnet)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-Web_API-512BD4.svg)
![MongoDB](https://img.shields.io/badge/Database-MongoDB-47A248.svg?logo=mongodb)
![Tests](https://img.shields.io/badge/Tests-xUnit-5C2D91.svg)

[About](#about) • [Features](#features) • [Architecture](#architecture) • [API](#api-overview) • [Getting Started](#getting-started) • [Tests](#tests) • [Structure](#repository-structure)

</div>

---

## About

EcoGuide is a backend project that helps users make more informed food choices. It provides product information from the **Open Food Facts** dataset and stores user profiles, preferences, and activity in MongoDB.

The API is built with **ASP.NET Core on .NET 8**. It combines a product data source with authenticated user services, including profile management and food preference data such as allergens, cuisine type, Nutri-Score, and Eco-Score preferences.

## Features

- Retrieve product information by barcode or list a sample of products.
- Manage user profiles and their food preferences.
- Keep user search, scan, and consumption activity in MongoDB.
- Protect user routes with **Auth0 JWT bearer authentication**.
- Send email through **SendGrid** when configured.
- Run a background task to clean up eligible unverified Auth0 users.
- Seed development user data when the EcoGuide users collection is empty.

## Architecture

| Component | Responsibility |
|-----------|----------------|
| **ASP.NET Core API** | HTTP endpoints, JWT authentication, CORS, and request handling |
| **Data layer** | MongoDB documents, DTOs, repositories, and development seeding |
| **Open Food Facts database** | Product source, stored in the `off.products` collection |
| **EcoGuide database** | Application user data, stored in the `ecoguide.users` collection |
| **Background service** | Scheduled maintenance of unverified Auth0 accounts |

## API Overview

The API listens on the local HTTPS URL shown by `dotnet run`. User endpoints require a valid Auth0 bearer token; product endpoints do not require authorization.

| Method | Route | Description |
|--------|-------|-------------|
| `GET` | `/api/products` | Return up to five products |
| `GET` | `/api/products/{productId}` | Retrieve a product by its identifier |
| `POST` | `/api/user/register` | Create an application user profile |
| `POST` | `/api/user/get` | Retrieve the authenticated user's profile |
| `POST` | `/api/user/update` | Update profile details and preferences |
| `PUT` | `/api/user/update/{userId}` | Update the user's email notification setting |
| `GET` | `/api/user/email/{email}` | Check whether an email address is registered |
| `GET` | `/api/user/logout` | Call the Auth0 logout endpoint |
| `DELETE` | `/api/user/delete/{userId}` | Delete a user profile |
| `POST` | `/api/user/send` | Send an email using the configured SendGrid integration |

Example product request:

```bash
API_BASE_URL=https://localhost:50623 # Replace with the HTTPS URL printed by dotnet run
curl --insecure "$API_BASE_URL/api/products/0000000000949"
```

`--insecure` is only for local development with an untrusted development certificate. Do not use it for production requests.

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) or Docker Engine with the Compose plugin
- An Open Food Facts MongoDB dump for product lookups
- Auth0 credentials for authenticated user endpoints
- A SendGrid API key if you want to use the email endpoint

### 1. Prepare MongoDB

The Compose configuration expects the Open Food Facts archive at the repository root. Download the dump and save it as `openfoodfacts-mongodbdump.gz`:

```text
https://static.openfoodfacts.org/data/openfoodfacts-mongodbdump.gz
```

The archive is intentionally excluded from Git because of its size. Start MongoDB from the repository root:

```bash
docker compose up -d
```

Restore the archive into MongoDB:

```bash
docker exec -i mongodb mongorestore --gzip --archive=/openfoodfacts-mongodbdump.gz
```

The full restore can take a significant amount of time and disk space. MongoDB stores its data in the local `.docker/data/db` directory.

### 2. Configure the application

The default MongoDB connection string is `mongodb://localhost:27017`. Provide or override settings through environment variables; for a public repository, do not commit credentials:

| Environment variable | Purpose |
|----------------------|---------|
| `ConnectionStrings__MongoDB` | MongoDB connection string |
| `Auth0__Domain` | Auth0 tenant domain |
| `Auth0__Audience` | API audience used to validate access tokens |
| `Auth0__ClientId` | Auth0 machine-to-machine application client ID |
| `Auth0__ClientSecret` | Auth0 machine-to-machine application secret |
| `Auth0__Audience_register` | Auth0 Management API audience used by the background service |
| `SENDGRIDKEY` | SendGrid API key used by the email integration |

Set Auth0 values for your own tenant and configure the required Auth0 Management API permissions for the background cleanup task. Never put real secrets in this README, source files, or commits.

### 3. Build and run

From the repository root:

```bash
dotnet restore EcoGuide.sln
dotnet build EcoGuide.sln
dotnet run --project ./src/Api/EcoGuide.Api.csproj
```

In Development, the application seeds sample user activity if the `ecoguide.users` collection is empty. Product requests return data only after the Open Food Facts archive has been restored.

## Tests

Run the solution's tests with:

```bash
dotnet test EcoGuide.sln
```

The repository includes API controller tests and API integration tests.

## Repository Structure

```text
eco-guide-api-bd-team-20/
├── src/
│   ├── Api/
│   │   ├── Controllers/           # Product and user HTTP endpoints
│   │   ├── DI/                    # MongoDB and data-service registration
│   │   ├── Filters/               # API exception handling
│   │   ├── Program.cs             # Application entry point
│   │   ├── Startup.cs              # Authentication, CORS, and middleware setup
│   │   └── CronDbService.cs        # Scheduled Auth0 account maintenance
│   └── Data/
│       ├── Documents/             # MongoDB document models
│       ├── Dtos/                  # API data transfer objects and score enums
│       ├── Repositories/          # Product and user data access
│       └── Seed/                  # Development sample-data seeder
├── tests/
│   ├── Api.Tests/                 # Controller unit tests
│   ├── Api.Integration.Tests/     # API integration tests
│   └── Data.Tests/                # Data-layer tests
├── httpreqs/                      # HTTP request examples
├── docker-compose.yml             # Local MongoDB service
├── EcoGuide.sln                   # .NET solution
└── README.md
```

## Contributors

Developed by **Team 20**. See the repository's GitHub contributors for individual contributions.

## License

No license file is currently included. Add a license before granting public reuse rights.

---

<div align="center">

**EcoGuide — making food product information easier to explore.**

</div>
