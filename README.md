# EcoGuide

## Description

EcoGuide aims to encourage its users to adopt a more sustainable lifestyle by providing them with an easy and interactive way to assess the environmental impact of their food consumption. Through the app, users can quickly access detailed product information by scanning them, get statistics on their consumption habits, receive suggestions for similar products tailored to their preferences, and much more.

## Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- [Docker](https://www.docker.com/get-started)
- [MongoDB Compass](https://www.mongodb.com/try/download/compass) or another MongoDB management tool

## Getting Started

### 1. Setup Database

EcoGuide uses the Open Food Facts database for product information. Follow these steps to set up the database:

- **Download the Open Food Facts MongoDB dump**: [openfoodfacts-mongodbdump.gz](https://static.openfoodfacts.org/data/openfoodfacts-mongodbdump.gz)

- **Place the downloaded .gz file at the root of the repository**

- **Set up the MongoDB container** using the existing `docker-compose.yml`. Make sure the name of the `.gz` file is identical to the name specified in the volumes section of the `.yml`.

  ```bash
  docker compose up
  ```

- **Access the terminal inside the container**.(You can also use Docker desktop for this):

  ```bash
    docker exec -it mongodb bash
  ```

- **Restore the database**. Run this command from the container (it may take ~45 minutes):
  ```bash
  mongorestore --gzip --archive=openfoodfacts-mongodbdump.gz
  ```

### 2. Build & Run Project

1. Restore dependencies & Build the solution:

   ```bash
   dotnet build
   ```

2. Run the project:
   ```bash
   dotnet run --project ./src/Api/EcoGuide.Api.csproj
   ```

#### Running Tests

To run tests, use the following command:

```bash
dotnet test
```

### Sending Requests to the API

#### Get details about a product

To retrieve details about a product, send a GET request to:

```
 https://localhost:50623/api/products/{productId}
```

### About databases

This application utilizes two MongoDB databases, both hosted on the same local server within a Docker container. The first database, `OpenFoodFact`, contains data related to various products. The second database, `EcoGuide`, stores data pertinent to the application itself, including user information. The EcoGuide database comes pre-initialized with sample data, as detailed in the `DataSeeder.cs` file.

You can configure the connection string for the database server in the `appsettings.Development.json` file.
