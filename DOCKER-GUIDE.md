# Local Docker Setup Guide

## Prerequisites

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and running
- .NET 10 SDK installed
- At least 6 GB RAM allocated to Docker Desktop  
  *(Docker Desktop → Settings → Resources → Memory)*

---

## Folder Structure

```
compose/
├── .env                           # passwords & ports (edit before first run)
├── docker-compose.infra.yml       # Kafka, Redis, SQL Server, Kafka UI, Elasticsearch, Kibana
└── docker-compose.services.yml    # Gateway + EmployeeManagement + CoreService + BankingOperations
src/
├── Gateway/
├── EmployeeManagementService/
├── CoreService/
├── BankingOperationsService/
└── Web/                           # Next.js frontend (run with `npm run dev`)
```

The two compose files are **independent projects** (`banking-infra` and `banking-services`) that share a fixed external Docker network (`banking-net`). Bring them up and down separately — infra first.

---

## First-Time Setup

### 1. Edit the `.env` file

Open `compose/.env` and change the defaults:

```env
SA_PASSWORD=Y0ur_strong!Passw0rd        # SQL Server SA password (min 8 chars, mixed case + number + symbol)
JWT_SECRET=dev-only-shared-secret-at-least-32-characters-long
```

> **SQL Server password rules:** min 8 chars, must include uppercase, lowercase, number, and symbol.  
> The defaults work as-is for local dev — just don't use them in production.

---

## Running the Stack

All commands run from the `compose/` folder:

```bash
cd compose
```

### Option A — Infra only (recommended while developing in Visual Studio)

Starts SQL Server, Kafka, Redis, Kafka UI, Elasticsearch, and Kibana. Services run locally from VS / `dotnet run`.

```bash
docker compose -f docker-compose.infra.yml up -d
```

### Option B — Full stack (infra + .NET services in Docker)

Start infra first, then services. Services reference `banking-net` as an external network, so infra must be up first or the services project will fail to start.

```bash
# 1. Infra (once, or after a fresh start)
docker compose -f docker-compose.infra.yml up -d

# 2. Services (build + start)
docker compose -f docker-compose.services.yml up -d --build
```

The `--build` flag rebuilds service images. Drop it on subsequent runs if code hasn't changed:

```bash
docker compose -f docker-compose.services.yml up -d
```

> **Why the split?** Infra is slow-moving (SQL Server, Kafka, ES) and you rarely recycle it. Services are fast-moving — you rebuild them constantly. Keeping them in separate compose projects means `docker compose -f docker-compose.services.yml down` tears down only the app containers, leaving your databases, Kafka topics, and ES indices untouched.

---

## Verifying Everything is Running

Each project has its own `docker compose ps`:

```bash
docker compose -f docker-compose.infra.yml ps
docker compose -f docker-compose.services.yml ps
```

All containers should show `healthy` or `running`:

```
NAME                        STATUS
banking-sqlserver           healthy
banking-redis               healthy
banking-kafka               healthy
banking-kafka-ui            running
banking-elasticsearch       healthy
banking-kibana              running
banking-gateway             running
banking-employee-management running
banking-core-service        running
banking-banking-operations  running
```

If something shows `starting`, wait 20–30 seconds and check again — SQL Server and Kafka take time on first boot.

---

## Service URLs

| Service | URL | Notes |
|---|---|---|
| Gateway | http://localhost:5000 | Entry point for all requests |
| EmployeeManagementService | http://localhost:5100 | Direct access (bypass gateway) |
| CoreService | http://localhost:5200 | Direct access (bypass gateway) |
| BankingOperationsService | http://localhost:5300 | Direct access (bypass gateway) |
| Kafka UI | http://localhost:8080 | Browse topics, messages, consumer groups |
| Kibana | http://localhost:5601 | Browse Serilog logs in Elasticsearch |
| Elasticsearch | http://localhost:9200 | Raw ES API (security disabled in dev) |
| SQL Server | localhost:1433 | User: `sa`, Password: from `.env` |
| Redis | localhost:6379 | No auth in dev |

### Swagger

Each service exposes Swagger in Development:

- http://localhost:5100/swagger — EmployeeManagementService
- http://localhost:5200/swagger — CoreService
- http://localhost:5300/swagger — BankingOperationsService

---

## Viewing Logs

```bash
# Infra
docker compose -f docker-compose.infra.yml logs -f
docker compose -f docker-compose.infra.yml logs -f kafka
docker compose -f docker-compose.infra.yml logs -f sqlserver

# Services
docker compose -f docker-compose.services.yml logs -f
docker compose -f docker-compose.services.yml logs -f gateway
docker compose -f docker-compose.services.yml logs -f employee-management
docker compose -f docker-compose.services.yml logs -f core-service
docker compose -f docker-compose.services.yml logs -f banking-operations
```

---

## Stopping the Stack

```bash
# Stop only services (keep infra + data running)
docker compose -f docker-compose.services.yml down

# Stop only infra (keep containers — but services will break if their deps are gone)
docker compose -f docker-compose.infra.yml down

# Full teardown (services first, then infra)
docker compose -f docker-compose.services.yml down
docker compose -f docker-compose.infra.yml down

# Full teardown + delete all data (fresh start)
docker compose -f docker-compose.services.yml down
docker compose -f docker-compose.infra.yml down -v
```

> Always stop services before infra — services depend on `banking-net`, which is owned by the infra project.

---

## Common Issues

### Services fail with "network banking-net not found"

Infra isn't running. Start it first:

```bash
docker compose -f docker-compose.infra.yml up -d
```

### SQL Server takes too long to start

If a service exits with a connection error on first boot, it just needs more time. Re-run:

```bash
docker compose -f docker-compose.services.yml up -d
```

Docker will restart any containers that exited while waiting.

### Port already in use

Edit `compose/.env` and change the conflicting port, e.g.:

```env
GATEWAY_PORT=5010
```

Then restart:

```bash
docker compose -f docker-compose.services.yml down
docker compose -f docker-compose.services.yml up -d
```

### Service shows as unhealthy

Check logs for that service:

```bash
docker compose -f docker-compose.services.yml logs employee-management
```

Most common cause: EF Core migration failed because SQL Server wasn't ready yet. Re-running `up -d` restarts the service and retries.

### Kafka UI can't connect

Kafka UI depends on Kafka being healthy. Give it 30–60 seconds after Kafka starts, then refresh http://localhost:8080.

---

## Rebuilding a Single Service

When you change code in one service, rebuild just that one:

```bash
docker compose -f docker-compose.services.yml up -d --build gateway
docker compose -f docker-compose.services.yml up -d --build employee-management
docker compose -f docker-compose.services.yml up -d --build core-service
docker compose -f docker-compose.services.yml up -d --build banking-operations
```

---

## Connecting to SQL Server from SSMS or Azure Data Studio

| Setting | Value |
|---|---|
| Server | `localhost,1433` |
| Authentication | SQL Server Authentication |
| Username | `sa` |
| Password | value from `.env` |
| Trust server certificate | ✓ checked |

Databases created automatically on first service startup:
- `Employee_DB` — EmployeeManagementService
- `Core_DB` — CoreService
- `BankingOperations_DB` — BankingOperationsService

---

## Kafka — Producing and Consuming

Kafka is accessible from the host at `localhost:29092` (external listener).  
Services inside Docker use `kafka:9092` (internal listener) — this is already configured via environment variables.

Kafka UI at http://localhost:8080 lets you browse topics, inspect messages, check consumer groups and lag, and produce test messages.

---

## Serilog → Elasticsearch → Kibana

All services ship logs to Elasticsearch via the Serilog Elasticsearch sink (ECS-formatted).

- Inside Docker: services write to `http://elasticsearch:9200` (overridden via `Serilog__WriteTo__2__Args__nodeUris` in `docker-compose.services.yml`).
- From Visual Studio / host: services write to `http://localhost:9200` (the value in `appsettings.json`).

### Index patterns per service

| Service | Index |
|---|---|
| Gateway | `gateway-logs-YYYY.MM.DD` |
| EmployeeManagementService | `employee-management-logs-YYYY.MM.DD` |
| CoreService | `core-service-logs-YYYY.MM.DD` |
| BankingOperationsService | `banking-operations-logs-YYYY.MM.DD` |

### First-time Kibana setup

1. Open http://localhost:5601
2. Menu → **Stack Management** → **Data Views** → **Create data view**
3. Name: `banking-logs`, Index pattern: `*-logs-*`, Timestamp field: `@timestamp`
4. Menu → **Discover** to browse logs

> **RAM note:** Elasticsearch uses ~512 MB heap. If Docker Desktop runs out of memory, bump its memory allocation or lower `ES_JAVA_OPTS` in `docker-compose.infra.yml`.

---

## Quick Reference

```bash
# Infra only (VS/local services)
docker compose -f docker-compose.infra.yml up -d

# Everything (infra + services, build images)
docker compose -f docker-compose.infra.yml up -d
docker compose -f docker-compose.services.yml up -d --build

# Status
docker compose -f docker-compose.infra.yml ps
docker compose -f docker-compose.services.yml ps

# Follow logs
docker compose -f docker-compose.services.yml logs -f gateway

# Stop services, keep infra + data
docker compose -f docker-compose.services.yml down

# Full teardown, keep data
docker compose -f docker-compose.services.yml down
docker compose -f docker-compose.infra.yml down

# Full teardown + wipe data
docker compose -f docker-compose.services.yml down
docker compose -f docker-compose.infra.yml down -v

# Rebuild one service
docker compose -f docker-compose.services.yml up -d --build employee-management
```
