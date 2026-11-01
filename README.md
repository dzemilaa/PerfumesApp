# Bella Perfumes

Perfume storefront built with React, Vite, ASP.NET Core, GraphQL, and PostgreSQL.
The repository includes five backend microservices and a frontend.

## Run locally

### Requirements

- Git
- Docker Desktop with Docker Compose

### Start the complete application

```powershell
git clone https://github.com/dzemilaa/PerfumesApp.git
cd PerfumesApp
Copy-Item .env.example .env
docker compose up --build
```

Open the frontend at <http://localhost:5174>. The first startup builds the five
API services, creates a local PostgreSQL database for each service, initializes
their schemas, and seeds three sample products. All database credentials used
by Compose are for this local development environment only.

To stop the services, press `Ctrl+C` and run:

```powershell
docker compose down
```

To remove the local database and start over, use `docker compose down -v`.
This permanently deletes the local Compose database and its data.

### Local email verification

Registration works without an email provider. In another terminal, run:

```powershell
docker compose logs -f loginservice
```

Open the `Local email verification URL` printed after registration. Real
SendGrid delivery and Cloudinary uploads require credentials provided only
through local environment variables or deployment secrets. Local product
uploads are stored by the product service container.

### Run checks

```powershell
cd client-frontend
npm ci
npm run build
```

Backend services can be built with the .NET 10 SDK:

```powershell
dotnet build mikroservisi/loginservice/loginservice.csproj
dotnet build mikroservisi/productservice/productservice.csproj
dotnet build mikroservisi/shopservice/shopservice.csproj
dotnet build mikroservisi/orderservice/orderservice.csproj
dotnet build mikroservisi/reviewservice/reviewservice.csproj
```

With the local stack running, the GraphQL API tests can be run with:

```powershell
cd mikroservisi/tests
npm ci
npm test
```

## Services

| Service | Local URL |
| --- | --- |
| Frontend | <http://localhost:5174> |
| Login GraphQL | <http://localhost:5000/graphql> |
| Product GraphQL | <http://localhost:5001/graphql> |
| Shop GraphQL | <http://localhost:5002/graphql> |
| Order GraphQL | <http://localhost:5003/graphql> |
| Review GraphQL | <http://localhost:5004/graphql> |

## Configuration and security

`appsettings.json` files contain non-sensitive defaults only. Database
connection strings, SendGrid keys, Cloudinary credentials, and deployment
configuration must be supplied outside Git. `.env` is ignored; `.env.example`
contains only local development defaults. Never put production credentials or
personal data into `.env.example`, source files, or commits.

The local database schemas and sample catalog are created by the services on
first start in Development mode. No real accounts, orders, or customer data
are included.
