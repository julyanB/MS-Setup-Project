# Local Development Guide

## What this guide is for

The whole stack normally runs in Docker. But when you're actively developing one of the services, you want to **debug it in Visual Studio** — see breakpoints, edit code, watch variables. Meanwhile you still want the rest of the stack (gateway, the other service, SQL, Kafka, Redis, Elasticsearch) running as before so nothing else breaks.

This guide explains how to wire that up. The mental model matters more than the commands, so we'll start there.

---

## The Mental Model — "Two Worlds"

There are two separate networking worlds on your machine:

### World 1 — **Your Windows host**
This is where Visual Studio, Postman, your browser, and DBeaver all run. When any of these programs say `localhost`, they mean "this PC".

### World 2 — **The Docker network (`banking-net`)**
Every container (`banking-gateway`, `banking-employee-management`, `banking-sqlserver`, …) lives inside this private network. They can talk to each other using their container names as DNS (`kafka`, `redis`, `sqlserver`, `employee-management`, `core-service`). To a container, `localhost` means **itself** — not your PC.

### The bridges between the two worlds

There are exactly two ways the worlds touch each other:

| Direction | Mechanism | Example |
|---|---|---|
| **Host → container** | Published ports in `.env` (`GATEWAY_PORT=5000`, `SQL_PORT=14330`, …). Docker forwards your host port to the container's internal port. | Postman on host calls `http://localhost:5000` → Docker forwards it to `banking-gateway:8080` inside. |
| **Container → host** | The special hostname `host.docker.internal`. Docker Desktop resolves this to your host machine's IP from inside any container. | Gateway container calls `http://host.docker.internal:5100` → reaches something listening on your PC's port 5100. |

**The key insight:** `localhost` means different things depending on who says it. Always ask yourself "who is making this call?" before writing an address.

---

## The "localhost" trap — worked example

Let's say Visual Studio is running EmployeeManagementService on `http://localhost:5100`. Postman, on your host, can reach it at `http://localhost:5100` just fine — both are in World 1.

Now you tell the gateway (which lives in World 2, inside Docker) to forward to `http://localhost:5100`. The gateway dutifully tries — but inside the container, `localhost` means the **container itself**, and nothing is listening on port 5100 inside the gateway container. Connection refused.

To make the gateway reach your Visual Studio process, it needs to *leave* World 2 and cross into World 1. The address for that is:

```
http://host.docker.internal:5100/
```

- `host.docker.internal` — "the Windows machine that's hosting me"
- `:5100` — the port your VS debugger picked

### Rule of thumb

| Who is making the call? | What "localhost" means | Use this for "the host PC" |
|---|---|---|
| Postman / browser on Windows | your PC | `localhost` |
| Visual Studio (host process) | your PC | `localhost` |
| A container (gateway, service, anything) | the container itself | **`host.docker.internal`** |

**When debugging, only the port changes** — the hostname always stays `host.docker.internal` from a container's perspective. If VS runs the service on 5100 today and 5123 tomorrow, you change `:5100` to `:5123` and nothing else.

---

## Host Ports (from `.env`)

Your VS-debugged service connects to these from the host, so they all use `localhost`:

| Infra | Host URL (World 1) | Container URL (World 2) |
|---|---|---|
| SQL Server | `localhost,14330` | `sqlserver,1433` |
| Redis | `localhost:6379` | `redis:6379` |
| Kafka | `localhost:29092` | `kafka:9092` |
| Elasticsearch | `http://localhost:9200` | `http://elasticsearch:9200` |
| Gateway (in Docker) | `http://localhost:5000` | — |
| EmployeeMgmt (in Docker) | `http://localhost:5100` | `http://employee-management:8080` |
| CoreService (in Docker) | `http://localhost:5200` | `http://core-service:8080` |
| BankingOps (in Docker) | `http://localhost:5300` | `http://banking-operations:8080` |

The committed `appsettings.json` already uses the World-1 values (`localhost:9200`, `localhost:29092`, etc.) — perfect for local debugging. The only thing that needs overriding is the SQL connection string, because the default points at a trusted local SQL Server (`Server=.`).

---

## Scenario A — Debug EmployeeManagementService locally, rest in Docker

### Step 1 — Stop only the containerized service

```bash
cd "Local docker setup/compose"
docker compose stop employee-management
```

Now port 5100 on your host is free (that's what Docker was publishing the container on). Everything else keeps running — SQL, Kafka, Redis, Elasticsearch, Kibana, Gateway, CoreService.

### Step 2 — Point your local service at the Dockerized infra

Your VS process is in World 1, so it uses host URLs.

Edit `src/EmployeeManagementService/EmployeeManagementService.Startup/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,14330;Database=Employee_DB;User Id=sa;Password=Y0ur_strong!Passw0rd;TrustServerCertificate=True;Encrypt=False"
  },
  "Kafka": {
    "BootstrapServers": "localhost:29092",
    "ClientId": "employee-management",
    "GroupId": "employee-management-service"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "ApplicationSettings": {
    "Secret": "dev-only-shared-secret-at-least-32-characters-long"
  }
}
```

> **Critical:** `ApplicationSettings.Secret` **must match** the `JWT_SECRET` value in `compose/.env`. The gateway signs tokens with that secret and your local service must validate with the same one. Mismatch = the gateway issues a JWT, your local service rejects every call as 401.

### Step 3 — Start the service in Visual Studio (F5)

When it starts, the console shows something like:

```
Now listening on: http://localhost:5100
Now listening on: https://localhost:7100
```

**Write down the HTTP port** (5100 here). We'll call it `<LOCAL_PORT>`. If your launchSettings.json uses a different port, that's fine — just use whatever it actually prints.

### Step 4 — Tell the Dockerized gateway to forward to your VS process

**Understanding what we're changing:**

The gateway has a config entry that says "when a request for the user-management-cluster comes in, forward it to this address." Right now it points to `http://employee-management:8080/` — which is the container (now stopped). We need it to point at your VS process instead.

Since the gateway is in World 2 (Docker) and your VS process is in World 1 (host), the gateway must reach it via `host.docker.internal`:

```
http://host.docker.internal:<LOCAL_PORT>/
```

**Open `compose/docker-compose.yml`**, find the `gateway` service, and change **only the address line** (leaving everything else alone):

Before:
```yaml
  gateway:
    environment:
      ...
      ReverseProxy__Clusters__user-management-cluster__Destinations__user-management-1__Address: http://employee-management:8080/
```

After (replace `5100` with your actual `<LOCAL_PORT>` if different):
```yaml
  gateway:
    environment:
      ...
      ReverseProxy__Clusters__user-management-cluster__Destinations__user-management-1__Address: http://host.docker.internal:5100/
```

Leave the `backend-cluster` line untouched — CoreService is still in Docker.

### Step 5 — Recreate the gateway

```bash
docker compose up -d gateway
```

Compose notices the environment changed, destroys and re-creates the gateway with the new forwarding address. ~2 seconds. Nothing else is affected.

### Step 6 — Test the flow

From a terminal on your host:

```bash
curl -X POST http://localhost:5000/identity/Register \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"test@local.dev\",\"password\":\"123456\"}"
```

What happens under the hood:

```
  curl (Postman, browser, whatever) on your host
       │
       ▼  World 1 → World 2 via published port 5000
  http://localhost:5000/identity/Register
       │
  ┌────▼────────────────────────────────┐
  │  Gateway container                  │
  │  matches "/identity/*" route        │
  │  → user-management-cluster          │
  │  → http://host.docker.internal:5100/│
  └────┬────────────────────────────────┘
       │
       ▼  World 2 → World 1 via host.docker.internal
  Your Windows machine, port 5100
       │
       ▼
  Visual Studio debugger 🎯 BREAKPOINT HITS
```

### Step 7 — When you're done debugging

Revert the line in `docker-compose.yml` back to `http://employee-management:8080/`, then restart both:

```bash
docker compose up -d employee-management gateway
```

Back to fully containerized.

---

## Scenario B — Debug CoreService locally

Identical pattern, different target cluster.

### Step 1
```bash
docker compose stop core-service
```

### Step 2 — `src/CoreService/CoreService.Startup/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,14330;Database=Core_DB;User Id=sa;Password=Y0ur_strong!Passw0rd;TrustServerCertificate=True;Encrypt=False"
  },
  "Kafka": {
    "BootstrapServers": "localhost:29092",
    "ClientId": "core-service",
    "GroupId": "core-service"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### Step 3
F5 in VS, note the HTTP port (e.g. `5200`).

### Step 4 — override the **backend-cluster** address this time

```yaml
  gateway:
    environment:
      ...
      ReverseProxy__Clusters__backend-cluster__Destinations__backend-1__Address: http://host.docker.internal:5200/
```

### Step 5
```bash
docker compose up -d gateway
```

### Step 6
Any non-`/identity/*` call through the gateway (e.g. `GET http://localhost:5000/api/anything`) will now hit your local CoreService.

---

## Scenario C — Debug BankingOperationsService locally

Same pattern as Scenario B, different cluster.

### Step 1
```bash
docker compose stop banking-operations
```

### Step 2 — `src/BankingOperationsService/BankingOperationsService.Startup/appsettings.Development.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost,14330;Database=BankingOperations_DB;User Id=sa;Password=Y0ur_strong!Passw0rd;TrustServerCertificate=True;Encrypt=False"
  },
  "Kafka": {
    "BootstrapServers": "localhost:29092",
    "ClientId": "banking-operations",
    "GroupId": "banking-operations-service"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  }
}
```

### Step 3
F5 in VS, note the HTTP port (e.g. `5300`).

### Step 4 — override the **banking-operations-cluster** address

```yaml
  gateway:
    environment:
      ...
      ReverseProxy__Clusters__banking-operations-cluster__Destinations__banking-operations-1__Address: http://host.docker.internal:5300/
```

### Step 5
```bash
docker compose up -d gateway
```

### Step 6
Any `/banking/*` call through the gateway (e.g. `GET http://localhost:5000/banking/anything`) will now hit your local BankingOperationsService.

---

## Scenario D — Debug the Gateway locally

The gateway itself becomes the World-1 process this time. The other two services stay in Docker on their published host ports.

### Step 1
```bash
docker compose stop gateway
```

### Step 2 — `src/Gateway/appsettings.Development.json`

The gateway is now running on your host (World 1), so it reaches the other services via `localhost:<host-port>`:

```json
{
  "ApplicationSettings": {
    "Secret": "dev-only-shared-secret-at-least-32-characters-long"
  },
  "ReverseProxy": {
    "Clusters": {
      "user-management-cluster": {
        "Destinations": {
          "user-management-1": { "Address": "http://localhost:5100/" }
        }
      },
      "backend-cluster": {
        "Destinations": {
          "backend-1": { "Address": "http://localhost:5200/" }
        }
      },
      "banking-operations-cluster": {
        "Destinations": {
          "banking-operations-1": { "Address": "http://localhost:5300/" }
        }
      }
    }
  }
}
```

### Step 3
F5 — gateway runs on whatever port `launchSettings.json` configures (not `:5000` anymore, since the container that used to publish on 5000 is stopped). Hit it at its VS-assigned HTTP port.

---

## Debugging Two Services Simultaneously

Stop both containerized services, run both in VS on different ports (VS will pick non-conflicting defaults), and override **both** cluster addresses in `docker-compose.yml`:

```yaml
ReverseProxy__Clusters__user-management-cluster__Destinations__user-management-1__Address: http://host.docker.internal:5100/
ReverseProxy__Clusters__backend-cluster__Destinations__backend-1__Address: http://host.docker.internal:5200/
```

Then `docker compose up -d gateway`.

---

## Quick Reference — "Where do I point things?"

| From | To | Use this address |
|---|---|---|
| Postman/host → Gateway (Docker) | gateway | `http://localhost:5000` |
| Postman/host → SQL (Docker) | sqlserver | `localhost,14330` |
| VS (host) → SQL (Docker) | sqlserver | `Server=localhost,14330` |
| VS (host) → Kafka (Docker) | kafka | `localhost:29092` |
| VS (host) → Redis (Docker) | redis | `localhost:6379` |
| VS (host) → Elasticsearch (Docker) | elasticsearch | `http://localhost:9200` |
| **Gateway container → VS process** | **your host PC** | **`http://host.docker.internal:<PORT>`** |
| Gateway container → EmployeeMgmt container | employee-management | `http://employee-management:8080/` |
| Gateway container → CoreService container | core-service | `http://core-service:8080/` |
| Gateway container → BankingOps container | banking-operations | `http://banking-operations:8080/` |
| Any container → SQL container | sqlserver | `Server=sqlserver,1433` |
| Any container → Kafka container | kafka | `kafka:9092` |

---

## Common Gotchas

### 1. JWT signing secret must match
The gateway signs JWTs with `JWT_SECRET` from `compose/.env`. Your local service validates them with `ApplicationSettings.Secret`. These must be byte-for-byte identical. If login works but every subsequent call is 401, this is almost always why.

### 2. `host.docker.internal` on plain Linux
On Docker Desktop (Windows/macOS) this just works. On a plain Linux Docker daemon, add to the gateway service:
```yaml
  gateway:
    extra_hosts:
      - "host.docker.internal:host-gateway"
```

### 3. HTTPS redirect loops
If the gateway proxies to your local `https://...` endpoint, YARP won't trust your dev cert. Use the **HTTP** port from your launchSettings when filling in `<LOCAL_PORT>`. You can still call the gateway over HTTP for local work.

### 4. SQL runs on 14330, not 1433
Because you have a Windows-installed SQL Server sitting on 1433. We moved Docker's SQL to 14330 in `.env`. Don't forget the port when configuring DBeaver or connection strings — `localhost,14330` (note the **comma**, not a colon — that's SQL Server convention).

### 5. Migrations
If you debug locally against the Dockerized DB and add a new EF migration, only your local run has executed it. Next time the containerized version of the service boots, it'll either run the same migration (fine) or complain about drift (not fine). Easiest: let one place own migration execution — typically the version that actually boots against a fresh DB.

### 6. Port 5100 already in use
After `docker compose stop employee-management`, the published host port 5100 is released — VS can grab it. If VS picks a different port anyway (check `launchSettings.json`), just use whatever port VS actually prints when you hit F5. The important thing is that the gateway's override matches the port VS is actually listening on.

### 7. Forgetting to revert
Once you're done debugging, the gateway will keep trying `host.docker.internal:5100`. If VS isn't running, all gateway requests will fail with connection refused. Revert `docker-compose.yml` and recreate gateway before handing the stack to someone else or walking away.
