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
├── .env                        # passwords & ports (edit before first run)
├── docker-compose.infra.yml    # Kafka, Redis, SQL Server, Kafka UI, Elasticsearch, Kibana
└── docker-compose.yml          # all services + infra
src/
├── Gateway/
├── EmployeeManagementService/
├── CoreService/
└── BankingOperationsService/
```

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
cd "Local docker setup/compose"
```

---

### Option A — Infra only (recommended while developing in Visual Studio)

Starts SQL Server, Kafka, Redis, and Kafka UI. Services run locally from VS.

```bash
docker compose -f docker-compose.infra.yml up -d
```

---

### Option B — Full stack (everything in Docker)

Builds images from source and starts all services + infra.

```bash
docker compose up -d --build
```

The `--build` flag rebuilds service images. Drop it on subsequent runs if code hasn't changed:

```bash
docker compose up -d
```

---

## Verifying Everything is Running

```bash
docker compose ps
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

If something shows `starting`, wait 20–30 seconds and run `docker compose ps` again — SQL Server and Kafka take time on first boot.

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
# All services
docker compose logs -f

# One service
docker compose logs -f employee-management
docker compose logs -f core-service
docker compose logs -f banking-operations
docker compose logs -f gateway
docker compose logs -f kafka
```

---

## Stopping the Stack

```bash
# Stop containers but keep data volumes
docker compose down

# Stop and delete all data (fresh start)
docker compose down -v
```

---

## Common Issues

### SQL Server takes too long to start

The services depend on SQL Server being healthy before they boot. If a service exits with a connection error, it just needs more time. Run:

```bash
docker compose up -d
```

Docker will restart any containers that exited while waiting.

### Port already in use

Edit `compose/.env` and change the conflicting port, e.g.:

```env
GATEWAY_PORT=5010
```

Then restart: `docker compose down && docker compose up -d`

### Service shows as unhealthy

Check logs for that service:

```bash
docker compose logs employee-management
```

Most common cause: EF Core migration failed because SQL Server wasn't ready yet. Running `docker compose up -d` again will restart the service and retry.

### Kafka UI can't connect

Kafka UI depends on Kafka being healthy. Give it 30–60 seconds after Kafka starts, then refresh http://localhost:8080.

---

## Rebuilding a Single Service

When you change code in one service, rebuild just that one instead of the whole stack:

```bash
docker compose up -d --build employee-management
docker compose up -d --build core-service
docker compose up -d --build banking-operations
docker compose up -d --build gateway
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

- Inside Docker: services write to `http://elasticsearch:9200` (overridden via `Serilog__WriteTo__2__Args__nodeUris` in `docker-compose.yml`).
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
# Start infra only
docker compose -f docker-compose.infra.yml up -d

# Start everything (build images)
docker compose up -d --build

# Check status
docker compose ps

# Follow logs
docker compose logs -f

# Stop (keep data)
docker compose down

# Stop (delete data — fresh start)
docker compose down -v

# Rebuild one service
docker compose up -d --build employee-management
docker compose up -d --build banking-operations
```
