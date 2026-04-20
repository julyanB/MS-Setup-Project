# Business Logic Guide

This document is the living reference for how business logic is structured in this template and how the authorization / permissions system works. Keep it up to date as the project grows.

---

## Table of Contents

1. [Architecture Overview — What Goes Where](#1-architecture-overview--what-goes-where)
2. [Permissions System — How It Works](#2-permissions-system--how-it-works)
3. [Adding a New Feature — Step by Step](#3-adding-a-new-feature--step-by-step)
4. [Protecting an Endpoint](#4-protecting-an-endpoint)
5. [Adding a New Permission](#5-adding-a-new-permission)
6. [ICurrentUser — Reading the Caller's Identity](#6-icurrentuser--reading-the-callers-identity)
7. [BL Practices — Rules and Conventions](#7-bl-practices--rules-and-conventions)

---

## 1. Architecture Overview — What Goes Where

The solution is split into five projects. Each one has exactly one job and can only reference projects below it.

```
CleanTemplate.Web          ← HTTP: controllers, request/response models, middleware
        │
CleanTemplate.Application  ← Use cases: commands, queries, service interfaces
        │
CleanTemplate.Domain       ← Business rules: entities, value objects, domain exceptions
        │
CleanTemplate.Infrastructure  ← External concerns: EF Core, Identity, JWT, Kafka, Redis, HTTP clients
        │
CleanTemplate.Startup      ← Composition root: registers everything, runs migrations, seeds data
```

**The one rule that matters most:** inner layers never reference outer ones. `Domain` knows nothing about EF Core. `Application` knows nothing about SQL Server or JWT. Dependencies always point inward.

### Layer responsibilities

| Layer | Owns | Does NOT own |
|---|---|---|
| **Domain** | Entities, value objects, domain exceptions, business invariants | Persistence, HTTP, Identity |
| **Application** | Use-case services, command/query models, interface contracts (`IIdentity`, `IRolePermissions`, etc.) | Implementations of those interfaces, DB queries |
| **Infrastructure** | EF DbContext, repositories, Identity (`UserManager`, `RoleManager`), JWT generation, Kafka producers, Redis cache | HTTP controller logic, business rules |
| **Web** | Controllers, request models, response models, middleware pipeline | Business logic, DB access |
| **Startup** | `Program.cs` — wires DI, runs migrations, seeds initial data | Everything else |

---

## 2. Permissions System — How It Works

The permissions system spans four components: the **database**, the **JWT token**, the **Gateway**, and the **service itself**. Each plays a distinct role.

### 2.1 The Database — Three Tables

All permission data lives in three tables in the service database:

```
Permissions
  ├── Id          int (PK, identity)
  ├── Name        nvarchar(128) UNIQUE  ← "roles.manage", "payments.read", ...
  ├── Description nvarchar(256) null
  └── CreatedAt   datetimeoffset

RolePermissions                    UserPermissions
  ├── RoleId      nvarchar(450) ──► AspNetRoles.Id (CASCADE DELETE)
  ├── PermissionId int ──────────► Permissions.Id (RESTRICT DELETE)
  └── PK (RoleId, PermissionId)    (same shape, FK to AspNetUsers.Id)
```

A permission name (`"roles.manage"`) exists **once** in `Permissions`. It can be bound to any number of roles via `RolePermissions` and/or granted directly to individual users via `UserPermissions`.

**Delete behaviour:**
- Delete a **role** → its `RolePermissions` rows cascade-drop automatically.
- Delete a **user** → their `UserPermissions` rows cascade-drop automatically.
- Delete a **permission** → blocked (Restrict) if any role or user still holds it. Unbind first.

### 2.2 Granting Permissions — The Admin API

Two controllers manage grants. Both require the caller to already hold `roles.manage`:

```
POST   /admin/roles/{roleName}/permissions      ← grant permission to a role
DELETE /admin/roles/{roleName}/permissions/{p}  ← revoke from a role
GET    /admin/roles/{roleName}/permissions      ← list a role's permissions

POST   /admin/users/{userId}/permissions        ← grant permission directly to a user
DELETE /admin/users/{userId}/permissions/{p}    ← revoke from a user
GET    /admin/users/{userId}/permissions        ← list a user's direct permissions

POST   /admin/roles                             ← create a new role
GET    /admin/roles                             ← list all roles
POST   /admin/users/{userId}/roles              ← assign a role to a user
DELETE /admin/users/{userId}/roles/{roleName}   ← remove a role from a user
```

Behind the controllers, `RolePermissionsService` and `UserPermissionsService` use a **get-or-create** pattern: when you grant `"payments.read"` and that name doesn't yet exist in `Permissions`, it is created on the fly. Permission names are the source of truth — there's no separate "register permission" step.

### 2.3 Login — JWT Emission

When a user logs in (`POST /identity/login`), `JwtTokenGeneratorService` builds the JWT:

```
1. Add NameIdentifier (user.Id) and Name (user.Email) claims.

2. Load roles from UserManager → add one ClaimTypes.Role per role.

3. Look up the user's role IDs, then query:
       RolePermissions ⋈ Permissions  WHERE RoleId IN (user's role IDs)
   → each result emitted as  claim type: "permission"

4. Query:
       UserPermissions ⋈ Permissions  WHERE UserId = user.Id
   → each result emitted as  claim type: "permission"   (gateway enforces it)
                        AND  claim type: "user_permission"  (for downstream distinction)
```

The JWT wire format never changes regardless of where the permissions are stored.  
`"permission": "roles.manage"` in the token means the same thing whether it came from a role or a direct grant.

### 2.4 Gateway — Config-Driven Enforcement

The Gateway validates the JWT and enforces authorization **before** forwarding the request. Two policy families exist:

**Static policies** — declared in code:
```csharp
options.AddPolicy("authenticated", p => p.RequireAuthenticatedUser());
```

**Dynamic `permission:` policies** — resolved by `PermissionPolicyProvider` at runtime:
```
"AuthorizationPolicy": "permission:payments.read"
```
The provider strips the prefix and builds `RequireAuthenticatedUser() + RequireClaim("permission", "payments.read")` on the fly. No code changes needed — policy names are resolved from the JWT at request time.

Set a route's policy in `appsettings.json`:
```json
"ReverseProxy": {
  "Routes": {
    "payments-route": {
      "AuthorizationPolicy": "permission:payments.read",
      "Match": { "Path": "/payments/{**catch-all}" },
      "ClusterId": "payments-cluster"
    }
  }
}
```
That's all. The Gateway rejects `403` before the request ever reaches the service.

### 2.5 Service-Level Re-Check — Defense in Depth

The admin endpoints inside the service also carry `[Authorize(Policy = "roles.manage")]`. The service has its own `PermissionPolicyProvider` (in `CleanTemplate.Infrastructure.Identity.Authorization`) that resolves any policy name as a permission claim check.

**Why both?**  
The Gateway is the public enforcement point for external traffic. The service re-checks because:
- East-west service-to-service calls may bypass the gateway entirely.
- Port-forwarding or infrastructure misconfiguration shouldn't open a backdoor.

This is belt-and-suspenders. Two independent checks against the same claim.

### 2.6 Header Propagation to Downstream Services

After the Gateway authorizes the request, YARP injects user context headers on the proxied request:

| Header | Value |
|---|---|
| `X-User-Id` | `NameIdentifier` claim |
| `X-User-Email` | `Name` claim |
| `X-User-Roles` | comma-separated role names |
| `X-User-Permissions` | comma-separated `permission` claim values |

Any inbound `X-User-*` headers are stripped first — clients cannot spoof these. Downstream services read them via `ICurrentUser` without touching the JWT.

### 2.7 Full Request Flow

```
Client
  │  Bearer JWT
  ▼
Gateway
  ├─ JwtBearer validates signature + expiry
  ├─ PermissionPolicyProvider resolves "permission:X"
  ├─ RequireClaim("permission", "X") checked against JWT
  │     fail → 403 (never reaches service)
  │     pass ↓
  ├─ Strip client X-User-* headers
  ├─ Inject X-User-Id, X-User-Email, X-User-Roles, X-User-Permissions
  └─ YARP forwards to service
              │
              ▼
         Service controller
         [Authorize(Policy = "roles.manage")]  ← re-checks via PermissionPolicyProvider
              │  pass ↓
         Application service
              │
         Business logic / DB
```

---

## 3. Adding a New Feature — Step by Step

### Step 1 — Domain (if new data is involved)

Create your entity in `CleanTemplate.Domain/Models/`. Inherit from `Entity<TId>` for identity-carrying entities. For value objects inherit `ValueObject`. Domain validation goes here as guard clauses or domain exceptions — not in the application layer.

```csharp
// Domain/Models/Payment.cs
public class Payment : Entity<Guid>
{
    public required string Reference { get; set; }
    public required decimal Amount { get; private set; }

    public void ApplyRefund(decimal amount)
    {
        if (amount > Amount)
            throw new DomainException("Refund cannot exceed payment amount.");
        Amount -= amount;
    }
}
```

### Step 2 — Application (use case contract + service)

Create a command/query model and a service class in `CleanTemplate.Application/Features/YourFeature/`.

```csharp
// Application/Features/Payments/Commands/CreatePayment/CreatePaymentCommand.cs
public record CreatePaymentCommand(string Reference, decimal Amount);

// Application/Features/Payments/Commands/CreatePayment/CreatePaymentService.cs
public class CreatePaymentService
{
    private readonly ICleanTemplateDbContext _db;

    public CreatePaymentService(ICleanTemplateDbContext db) => _db = db;

    public async Task<ActionResult> Handle(CreatePaymentCommand command, CancellationToken ct)
    {
        var payment = new Payment { Reference = command.Reference, Amount = command.Amount };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);
        return new OkResult();
    }
}
```

**Rules for the Application layer:**
- Only depends on `Domain` and its own interfaces.
- No EF Core, no `HttpContext`, no `UserManager`.
- `ICurrentUser` is injected when you need the caller's identity — never read headers manually.
- Validation of inputs goes here (e.g. FluentValidation). Business rule violations go in `Domain`.

### Step 3 — Infrastructure (EF configuration)

Add the EF configuration and DbSet.

```csharp
// Infrastructure/Persistence/Configurations/PaymentConfiguration.cs
public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Reference).IsRequired().HasMaxLength(64);
        builder.Property(x => x.Amount).HasColumnType("decimal(18,2)");
    }
}
```

Add the DbSet to `CleanTemplateDbContext` and to `ICleanTemplateDbContext`.

### Step 4 — Web (controller)

Create the controller in `CleanTemplate.Web/Features/`. Use `[FromServices]` injection for application services — controllers are thin wrappers.

```csharp
[ApiController]
[Route("payments")]
[Authorize(Policy = "payments.create")]   // ← permission name
public class PaymentsController : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create(
        CreatePaymentCommand command,
        [FromServices] CreatePaymentService service,
        CancellationToken ct)
        => await service.Handle(command, ct);
}
```

### Step 5 — Migration

```bash
dotnet ef migrations add AddPaymentsTable \
  --project CleanTemplate.Infrastructure \
  --startup-project CleanTemplate.Startup
```

---

## 4. Protecting an Endpoint

There are two layers of protection. Use both.

### Layer 1 — Gateway (appsettings.json)

Add the route with a permission policy in the Gateway's `appsettings.json`:

```json
"Routes": {
  "payments-create": {
    "AuthorizationPolicy": "permission:payments.create",
    "Match": { "Path": "/payments", "Methods": ["POST"] },
    "ClusterId": "your-service-cluster"
  }
}
```

No code change. The `permission:` prefix is resolved dynamically.

### Layer 2 — Service controller (defense in depth)

```csharp
[Authorize(Policy = "payments.create")]
public class PaymentsController : ControllerBase { ... }
```

The service's `PermissionPolicyProvider` resolves `"payments.create"` as `RequireClaim("permission", "payments.create")`.

### Open endpoints (no auth)

For endpoints that need no authentication (login, register, health checks):

```csharp
[AllowAnonymous]
public async Task<ActionResult> Login(...) { ... }
```

In the Gateway, omit `AuthorizationPolicy` or set it to `null`:
```json
"auth-login": {
  "Match": { "Path": "/identity/login" },
  "ClusterId": "your-service-cluster"
}
```

---

## 5. Adding a New Permission

A permission name is just a string. There is no global registry — names are created on first use.

### Convention

Use `{domain}.{action}` naming:

| Name | Meaning |
|---|---|
| `roles.manage` | Create roles, assign permissions (seeded on startup for Admin) |
| `payments.read` | View payments |
| `payments.create` | Create a new payment |
| `payments.refund` | Issue a refund |
| `reports.export` | Export data to CSV/Excel |

Keep names lowercase with dots. Avoid spaces and special characters — they go in JWT claims.

### Granting a new permission to a role

After deployment, call the Admin API (requires a JWT with `roles.manage`):

```http
POST /admin/roles/Admin/permissions
Content-Type: application/json

{ "permission": "payments.create" }
```

If `"payments.create"` doesn't exist in the `Permissions` table yet, it is created automatically.

### Granting directly to a user (override)

```http
POST /admin/users/{userId}/permissions
Content-Type: application/json

{ "permission": "reports.export" }
```

This user now holds `"reports.export"` regardless of their role. It appears in their JWT as both a `"permission"` claim and a `"user_permission"` claim.

### Seeding a permission on startup

If a permission must exist from day one (like `roles.manage` for Admin), add it in `Program.cs`:

```csharp
// Program.cs — inside the migration scope
var permission = await db.Permissions
    .FirstOrDefaultAsync(p => p.Name == "payments.create");

if (permission is null)
{
    permission = new Permission
    {
        Name = "payments.create",
        Description = "Create new payments",
        CreatedAt = DateTimeOffset.UtcNow
    };
    db.Permissions.Add(permission);
    await db.SaveChangesAsync();
}

var role = await roleManager.FindByNameAsync("PaymentsAdmin");
if (role != null)
{
    var alreadyBound = await db.RolePermissions
        .AnyAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);

    if (!alreadyBound)
    {
        db.RolePermissions.Add(new RolePermission
        {
            RoleId = role.Id,
            PermissionId = permission.Id
        });
        await db.SaveChangesAsync();
    }
}
```

---

## 6. ICurrentUser — Reading the Caller's Identity

`ICurrentUser` is the single place to read who is calling. Inject it in Application services or Infrastructure code that needs to know the caller.

```csharp
public interface ICurrentUser
{
    bool IsAuthenticated { get; }
    string? UserId { get; }
    string? Email { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<string> UserPermissions { get; }  // direct grants only
}
```

The implementation reads the `X-User-*` headers injected by the Gateway. It does **not** touch the JWT — the Gateway already validated it.

`UserPermissions` contains only **direct user grants** (from `UserPermissions` table). Role-derived permissions are in the JWT but not here — they come from `X-User-Permissions` which includes both. If you need to check whether the caller has *any* permission (role-derived or direct), check `X-User-Permissions` via the policy system rather than `ICurrentUser.UserPermissions`.

**Typical usage in an application service:**

```csharp
public class CreatePaymentService
{
    private readonly ICurrentUser _currentUser;
    private readonly ICleanTemplateDbContext _db;

    public CreatePaymentService(ICurrentUser currentUser, ICleanTemplateDbContext db)
    {
        _currentUser = currentUser;
        _db = db;
    }

    public async Task Handle(CreatePaymentCommand command, CancellationToken ct)
    {
        var payment = new Payment
        {
            Reference = command.Reference,
            CreatedBy = _currentUser.UserId   // audit stamp — who created this
        };
        _db.Payments.Add(payment);
        await _db.SaveChangesAsync(ct);
    }
}
```

`DbContext.ApplyAuditStamps()` also reads `ICurrentUser` automatically for entities that implement `IAuditable` or `ITrackable`, so you often don't need to set `CreatedBy` manually.

---

## 7. BL Practices — Rules and Conventions

### Where business rules live

| Rule type | Lives in |
|---|---|
| "A refund cannot exceed the original amount" | **Domain** — guard clause in the entity method |
| "A user can only have 5 active payments" | **Domain** — domain service or entity invariant |
| "Send a Kafka event after payment created" | **Application** — orchestrated in the use-case service |
| "The caller must have payments.create" | **Web/Gateway** — `[Authorize(Policy = ...)]` + Gateway config |
| "Hash the password before saving" | **Infrastructure** — Identity `UserManager` handles this |

**Never put business logic in controllers.** Controllers translate HTTP → command → response. They call the application service and return the result. That's it.

**Never put business logic in EF configurations or DbContext.** Those are persistence concerns.

### Exceptions

| Exception type | Where to throw | Effect |
|---|---|---|
| `DomainException` | Domain layer | Maps to 400 Bad Request via the exception handler middleware |
| `NotFoundException` | Application or Infrastructure | Maps to 404 |
| `IdentityException` | Infrastructure (Identity) | Maps to 400 |
| `DatabaseException` | DbContext (auto-wrapped) | Maps to 409 Conflict (unique violation) or 503 (deadlock) |

Throw the most specific exception. Never throw `Exception` directly from business code.

### Validation

- **Input shape** (required fields, max length, format): FluentValidation in the Application layer, on the command/query model.
- **Business rules** (invariants that depend on domain state): Domain layer methods or domain services.
- **Idempotency guards** (already exists, already bound): Application service before writing, using `AnyAsync` — return early rather than throw.

### Commands vs Queries

Commands mutate state. Queries read state. Keep them separate:

```
Features/
  Payments/
    Commands/
      CreatePayment/
        CreatePaymentCommand.cs
        CreatePaymentService.cs
        CreatePaymentCommandValidator.cs
    Queries/
      GetPayment/
        GetPaymentQuery.cs
        GetPaymentService.cs
        PaymentOutputModel.cs
```

Query services are read-only — they never call `SaveChangesAsync`.

### Concurrency

Use optimistic concurrency via EF's `[Timestamp]` / `IsRowVersion()` for entities that may be updated concurrently. The `DbContext` maps `DbUpdateConcurrencyException` → `DatabaseException` with `ErrorCodes.ConcurrencyError` → 409. The client retries.

### Never expose entity objects from controllers

Controllers return output models (DTOs), not EF entities. Exposing entities leaks schema, causes circular-reference serialization issues, and tightly couples your API contract to your DB model.

```csharp
// Good
return Ok(new PaymentOutputModel(payment.Id, payment.Reference, payment.Amount));

// Bad
return Ok(payment);  // ← leaks EF navigation properties, schema details
```

---

*Last updated: 2026-04-20*
