# Banking Platform — Microservice Modernization Plan

**.NET 10 · ASP.NET · YARP Gateway · Clean Architecture · MediatR · Event-Driven**

---

## I. Coding Practices & Engineering Standards

### Mandatory from Day One

- **Clean Architecture per service** — each microservice follows a consistent 4-layer structure:
  - **API layer** — controllers or minimal APIs, request/response DTOs, middleware configuration. This is the entry point. No business logic here.
  - **Application layer** — MediatR handlers (one handler per use case), FluentValidation validators, service interfaces, mapping profiles. All orchestration logic lives here.
  - **Domain layer** — entities, enums, constants, shared business rules. Pure C# — no infrastructure dependencies, no NuGet packages beyond what's needed for the models themselves.
  - **Infrastructure layer** — EF Core DbContext, repository implementations, external API clients, email/SMS providers, file storage. Everything that touches I/O.

- **MediatR for use case organization** — every API endpoint dispatches a request (command or query object) to a MediatR handler. One handler = one use case = one file. This keeps controllers thin and business logic testable in isolation. Example: `CreateLeaveRequest` → `CreateLeaveRequestHandler` → returns `Result<LeaveResponseDto>`.

- **Strong input validation with FluentValidation** — every MediatR request gets a validator registered in the pipeline. No optional-everything DTOs. The API contract must explicitly define what is required vs optional. The frontend must know exactly what to send. Use a MediatR pipeline behavior to run validation automatically before the handler executes.

- **Repository pattern with Unit of Work** — EF Core DbContext as UoW, repository abstractions per entity/aggregate. No raw DbContext injection into handlers — handlers depend on repository interfaces defined in the Application layer, implemented in Infrastructure.

- **Result pattern (no throwing exceptions for business logic)** — use a `Result<T>` type for all handler responses. Exceptions are for exceptional things (infrastructure failures), not validation or business rule violations. The API layer maps `Result<T>` to appropriate HTTP status codes.

- **Dependency injection everywhere** — all services, repositories, and external clients registered via DI. No static classes, no service locator pattern. This makes everything testable and swappable.

- **Consistent project template** — create a solution template (dotnet new template or a template repo) that every new service is scaffolded from. Same folder structure, same NuGet packages, same middleware pipeline, same logging setup. No service is a snowflake.

### Transaction Discipline

- Every write operation wrapped in an explicit transaction with the correct isolation level.

- **READ COMMITTED** as the default — safe, no dirty reads, good enough for most operations.

- **REPEATABLE READ** for operations that read-then-write based on the read data (e.g., balance checks before transfers, approval workflows where you must ensure the state hasn't changed between read and write).

- **SERIALIZABLE** only for critical financial flows where absolute consistency is required (e.g., payment execution, loan disbursement, account balance modifications).

- **READ UNCOMMITTED — NEVER use this in the new system.** It was a temporary fix in the old project to reduce database locking and must not carry over. If you need read performance, use read replicas or optimized projections instead.

- Rollback on failure is automatic when using transactions correctly — no more partial writes persisting after errors.

### Database Access Rules

- **Projection-first querying** — never `SELECT *` or load entire entities when you only need 3 fields. Use `.Select()` projections in EF Core for all read queries.

- **No loading entire tables into memory** — if you need aggregation, do it in SQL, not in C# LINQ-to-Objects after materializing the full table.

- **Pagination mandatory** for any list endpoint — no endpoint returns unbounded result sets. Use keyset pagination (`WHERE Id > @lastId ORDER BY Id`) for consistent performance on large datasets.

- **`AsNoTracking()`** on all read queries — reduces memory pressure and improves performance since EF Core skips change tracking overhead.

- **Compiled queries** for hot paths — EF Core compiled queries eliminate expression tree compilation overhead on frequently called endpoints.

- **Connection pool sizing per service** — each microservice has its own connection pool sized for its expected load, eliminating cross-service pool exhaustion that plagued the monolith.

### Parallelism & Concurrency Rules

- **No unbounded `Task.WhenAll` / `Parallel.ForEach`** — always limit concurrency with `SemaphoreSlim` or `Channel<T>`. Example: if you need to call 50 external APIs, limit to 10 concurrent calls.

- Use async/await everywhere, but don't parallelize unless the operation is genuinely I/O-bound and benefits from it. CPU-bound work in a web server should stay sequential to avoid thread pool starvation.

- **Thread pool starvation monitoring** — add metrics for `ThreadPool.PendingWorkItemCount` and alert when it exceeds thresholds. This was the likely cause of slowdowns in the old system.

- **Connection pool exhaustion monitoring** — track active/idle connections per service and alert before hitting max pool size.

- **HttpClient management** — use `IHttpClientFactory` with named/typed clients. Never `new HttpClient()` directly. Configure timeouts and retry policies via Polly.

---

## II. Testing Strategy

### Test Pyramid — Enforced, Not Optional

- **Unit Tests (xUnit + NSubstitute/Moq)** — every MediatR handler gets unit tests. Mock the repository interfaces, test business logic in isolation. Target: cover all business rules and edge cases, not arbitrary line coverage.

- **Integration Tests (WebApplicationFactory + Testcontainers)** — spin up real database containers (SQL Server in Docker), test the full pipeline from HTTP request through MediatR handler to database and back. Every endpoint gets at least one happy-path and one error-path integration test.

- **Contract Tests** — validate that the API contract (request/response shapes) doesn't break between versions. Use schema validation or Pact-style consumer-driven contracts between services.

- **Load Tests (k6 or NBomber)** — before any service goes to production, run load tests simulating expected peak traffic. Establish baseline response times and throughput. Run again on every major release.

- **Regression Tests** — when QA finds a bug, the developer writes a test that reproduces it before fixing it. That test stays in the suite forever.

### Enforcement Model

- **CI pipeline blocks merges if tests fail** — no exceptions, no "I'll fix it later."

- **No blind coverage targets (like 80%)** — instead, require that every PR includes tests for the functionality it adds or changes. Code review must verify this.

- **QA scenarios become test cases** — after QA tests a feature, their test scenarios are documented and converted into automated integration tests by the developer. This enriches the regression suite continuously.

- **Test naming convention:** `MethodName_Scenario_ExpectedResult` (e.g., `CreateLeaveRequest_WhenStartDateInPast_ReturnsValidationError`).

---

## III. Microservice Architecture — Consolidated 7-Service Split

### Design Principles

- Group by **business domain ownership**, not by technical function. Each service owns its data, workflows, and lifecycle independently.
- **Fewer, larger services** reduce operational overhead (deployment pipelines, monitoring dashboards, team cognitive load) while still providing isolation where it matters.
- **Rule of thumb:** if two request types share the same database tables and the same business rules, they belong in the same service. If they don't, they shouldn't be forced together.
- Each service has its **own database** — no shared databases between services.
- Every service follows the **same Clean Architecture template** — API → Application (MediatR handlers) → Domain (entities) → Infrastructure (EF Core, external clients).

---

### Service 1: API Gateway (YARP on .NET 10)

*Not a microservice — this is infrastructure.*

- Single entry point for all client traffic
- Routes `POST /api/auth/login` and `POST /api/auth/refresh` to EmployeeManagementService — token issuance is not the gateway's responsibility
- JWT validation on all non-auth requests (validates signature, rejects expired/invalid tokens with 401)
- Forwards validated claims (userId, roles, tenantCode, department) to downstream services as headers
- Coarse-grained authorization (is this user allowed to call `/api/banking/*`?)
- Tenant resolution (BG/RO/GR) — extracted from JWT, injected as `X-Tenant-Id` header
- API versioning (`/v1/employee/leave` → EmployeeManagementService v1, `/v2/employee/leave` → v2)
- Correlation ID generation (`X-Correlation-Id`) — propagated to all downstream calls
- Rate limiting (per-tenant, per-endpoint)
- Request/response logging (method, path, status, duration)
- Health check routing — stops sending traffic to unhealthy service instances
- **No database. No business logic. No token issuance. Stateless.**

---

### Service 2: EmployeeManagementService — 20 Requests + Authentication & Authorization

**Domain:** Employee lifecycle, HR operations, workplace logistics — and the single source of truth for all authentication and authorization in the system.

#### Why Authentication Lives Here

This system is used exclusively by bank employees — there are no external users, no customers logging in. Authentication is 100% about employees. EmployeeManagementService already owns everything needed for auth: employee credentials, roles, department, tenant, and active/suspended status. Putting auth here means no data duplication, no cross-service calls at login, and a single place to manage employee identity.

#### Authentication Flow

```
1. Employee calls POST /api/auth/login (routed to EmployeeManagementService by the gateway)
2. EmployeeManagementService validates credentials against its own database
3. Checks employee isActive status — suspended/terminated employees are rejected immediately
4. Builds JWT claims: userId, email, roles, tenantCode, department, isActive
5. Issues signed JWT + refresh token
6. All subsequent requests → Gateway validates JWT signature → forwards claims to downstream services
7. No service ever calls EmployeeManagementService again at runtime — the JWT is self-contained
```

#### Auth Endpoints

| Endpoint | Description |
|---|---|
| `POST /api/auth/login` | Validate credentials, issue JWT + refresh token |
| `POST /api/auth/refresh` | Issue new JWT using a valid refresh token |
| `POST /api/auth/logout` | Invalidate refresh token |
| `GET /api/auth/my-permissions` | Return the employee's fully resolved permission set (roles + user overrides) for frontend UI |
| `POST /api/auth/change-password` | Employee changes their own password |

#### Important: Token Validation is at the Gateway

EmployeeManagementService is only called at **login and token refresh**. After that:
- The gateway validates the JWT signature on every request using the shared signing key
- Valid claims are forwarded to downstream services as headers
- **EmployeeManagementService is not involved in request validation at runtime** — no bottleneck, no single point of failure for in-flight requests

#### What Happens When an Employee is Suspended

When an `EmployeeState` request marks an employee as suspended or terminated:
- The employee's record is updated in the database (`isActive = false`)
- Their refresh tokens are immediately invalidated — they cannot get a new JWT
- Their current JWT remains valid until expiry (keep JWT expiry short — 15–30 minutes)
- After expiry, the next refresh attempt is rejected

#### Request Types

| # | Request Type | Description |
|---|---|---|
| 1 | NewEmployee | Onboarding a new employee into the system |
| 2 | Complaint | Employee complaint filing and resolution workflow |
| 3 | Leave | Annual/personal leave request and approval |
| 4 | BusinessTrip | Business travel request, approval, and expense tracking |
| 5 | Change | General employee data change request |
| 6 | Vehicle | Company vehicle assignment and management |
| 7 | Stamp | Official stamp request and issuance |
| 8 | BusinessCard | Business card ordering for employees |
| 9 | WorkWear | Work uniform/clothing requests |
| 10 | HotDesk | Hot desk booking and reservation |
| 11 | ChangePersonalData | Employee personal data modification (address, bank details, etc.) |
| 12 | HotParking | Parking spot booking and reservation |
| 13 | SickLeave | Sick leave submission with medical documentation |
| 14 | Covid | Covid-related leave and reporting |
| 15 | Certificate | Employment certificate generation |
| 16 | IdentificationDeclaration | Employee identity document submission |
| 17 | PhoneProblem | Company phone/device issue reporting |
| 18 | ExtendedWorkingTime | Overtime/extended hours request and approval |
| 19 | EmployeeState | Employee status change (active, suspended, terminated) — also triggers auth invalidation |
| 20 | ExpenseRequest | Employee expense submission and reimbursement |

**Why this grouping works:** All 20 request types share the employee master data (name, department, manager, contract terms, organizational hierarchy). Auth lives here for the same reason — it needs the same data. A leave request needs to know the employee's remaining balance and their manager for approval, the same way a login needs to know their roles and active status. One database, one set of entities, one service that knows everything about an employee.

**MediatR organization:** Feature folders — `Features/Auth/`, `Features/Leave/`, `Features/BusinessTrip/`, etc. Each feature is self-contained with its own handler and validator.

---

### Service 3: BankingOperationsService — 55 Requests

**Domain:** All banking work performed by employees on behalf of bank customers — financial transactions, customer data management, cards, merchants, and documents

| # | Request Type | Sub-Domain | Description |
|---|---|---|---|
| 1 | PaymentExecution | Payments | Execute an electronic payment |
| 2 | DirectDebit | Payments | Set up or process a direct debit |
| 3 | UnrecognizedPayment | Payments | Investigate and resolve unmatched payments |
| 4 | PaymentAccountOpening | Payments | Open a new payment account |
| 5 | RefundOverpaid | Payments | Process refund for overpayment |
| 6 | SWIFT | Payments | International SWIFT payment processing |
| 7 | CashDeskRevision | Cash Ops | Cash desk balance revision and reconciliation |
| 8 | CashDeskCoinExchange | Cash Ops | Coin exchange at cash desk |
| 9 | CashDeskCashIn | Cash Ops | Cash deposit at cash desk |
| 10 | CashDeskCashOut | Cash Ops | Cash withdrawal at cash desk |
| 11 | OperationalDeskCashIn | Cash Ops | Operational desk cash intake |
| 12 | OperationalDeskCashOut | Cash Ops | Operational desk cash disbursement |
| 13 | MainOperationalDeskTransfer | Cash Ops | Transfer between main operational desks |
| 14 | InkassoCashIn | Cash Ops | Inkasso cash collection intake |
| 15 | InkassoCashOut | Cash Ops | Inkasso cash disbursement |
| 16 | DepositOpening | Deposits | Open a new term/savings deposit |
| 17 | RaisinDepositOpening | Deposits | Open a Raisin platform deposit |
| 18 | AccountClosing | Deposits | Close an existing account/deposit |
| 19 | Check24Deposit | Deposits | Check24 platform deposit opening |
| 20 | FullEarlyPrepayment | Lending | Complete early repayment of a loan |
| 21 | PartialRepayment | Lending | Partial early repayment reducing principal |
| 22 | LoanApplicationReview | Lending | New loan application assessment and approval |
| 23 | LoanRepaymentReporting | Lending | Reporting on loan repayment status and schedules |
| 24 | RecurringLoanPayment | Lending | Scheduled recurring loan payment processing |
| 25 | ThirdPartyLoanInstallmentPayment | Lending | Third-party installment payment on behalf of borrower |
| 26 | NeonCreditRepayment | Lending | Neon credit product repayment processing |
| 27 | AutoLoanCertificate | Lending | Auto loan certificate generation for vehicle registration |
| 28 | AnnexZeroPercentageMarkup | Lending | Annex for zero-percentage markup loan modifications |
| 29 | NewCustomer | Customer Data | Register a new retail customer |
| 30 | NewExternalCustomer | Customer Data | Register a new external/corporate customer |
| 31 | Leads | Customer Data | Sales lead tracking and conversion |
| 32 | ChangeCustomerSector | Customer Data | Change customer's business sector classification |
| 33 | ChangeCustomerTransactionalLimits | Customer Data | Modify customer transaction limits |
| 34 | DeceasedCustomer | Customer Data | Handle deceased customer account processing |
| 35 | OnlineBanking | Customer Access | Online banking enrollment and management |
| 36 | ResetStaticPassword | Customer Access | Reset customer's static password |
| 37 | ResetDigitalBankingPassword | Customer Access | Reset customer's digital banking password |
| 38 | AccessAndAuthentication | Customer Access | Customer access rights and authentication management |
| 39 | DebitCardIssuing | Cards | Issue a new debit card for customer |
| 40 | CardCurrentStatus | Cards | Check/update current card status (active, blocked, etc.) |
| 41 | DigitalSigning | Cards | Digital signature enrollment and management |
| 42 | SmallMerchantRegistration | Merchants | Register a small merchant for card acceptance |
| 43 | MerchantAnnex | Merchants | Merchant contract annex and modifications |
| 44 | ActivateDeactivateSchemes | Merchants | Activate or deactivate payment schemes for merchant |
| 45 | IncomingCorrespondence | Documents | Register and route incoming correspondence |
| 46 | OutgoingCorrespondence | Documents | Create and send outgoing correspondence |
| 47 | IncomingCorrespondenceAutomation | Documents | Automated processing of incoming correspondence |
| 48 | OutgoingCorrespondenceAutomation | Documents | Automated generation of outgoing correspondence |
| 49 | IncomingCorrespondenceAuditConfirmation | Documents | Audit confirmation for incoming correspondence |
| 50 | AnswersToInstitutions | Documents | Generate and send responses to regulatory institutions |
| 51 | AnswersToCustomers | Documents | Generate and send responses to customer inquiries |
| 52 | ContractCopy | Documents | Generate contract copy for customer |
| 53 | ContractCancellation | Documents | Process contract cancellation documentation |
| 54 | PowerOfAttorney | Documents | Power of attorney document management |
| 55 | BankReference | Documents | Generate bank reference letters |

**Why this grouping works:** This is an internal bank portal used exclusively by bank employees. Every request in this service represents an employee performing banking work on behalf of a customer — processing their payments, managing their account data, issuing their cards, handling their documents. The "customer" here is the bank's client data that employees manage, not a separate user of the system. Keeping all of this in one service means the employee's full context — customer data, transaction history, documents, cards — is available without cross-service calls.

**Internal organization:** Feature folders within the Application layer: `Features/Payments/`, `Features/CashOperations/`, `Features/Deposits/`, `Features/Lending/`, `Features/CustomerData/`, `Features/Cards/`, `Features/Merchants/`, `Features/Documents/`.

---

### Service 4: ComplianceService — 9 Requests

**Domain:** Audit, risk, regulatory compliance, external system integrations

| # | Request Type | Sub-Domain | Description |
|---|---|---|---|
| 1 | RiskEvent | Risk | Log and assess risk events |
| 2 | ConflictOfInterest | Risk | Conflict of interest declaration and review |
| 3 | InternalAuditRecommendation | Audit | Internal audit findings and recommendations |
| 4 | InternalAuditAssignment | Audit | Assign audit tasks to team members |
| 5 | InsuranceCancellation | Compliance | Process insurance policy cancellation |
| 6 | Incident | Compliance | Incident reporting and tracking |
| 7 | Treasury | Integration | Treasury system integration and operations |
| 8 | MRelTreasury | Integration | MRel treasury integration operations |
| 9 | Navigator | Integration | Navigator external system integration |

**Why merged:** Compliance, audit, risk, and external integrations all share a common characteristic — they exist for regulatory oversight and external system coordination. Audit findings lead to risk assessments, risk events trigger compliance reporting, and treasury/external integrations are subject to regulatory audit. The integration endpoints (Treasury, MRelTreasury, Navigator) act as the anti-corruption layer for external systems, and their audit trail requirements align naturally with the compliance domain.

**Internal organization:** Feature folders: `Features/Risk/`, `Features/Audit/`, `Features/Integrations/`.

---

### Cross-Cutting Services (Infrastructure, Not Business)

#### CoreService

- Combines **orchestration** and **communication** — two event consumers that both sit on the message bus with no business domain of their own, merged into one service
- Owns the **RequestMetaData** table (see Section IV) — consumes domain events from all 4 business services and maintains the aggregated request view
- Exposes dashboard, search, and reporting endpoints — **role/policy-based authorization on every query** (see Section IV for full authorization design)
- Handles all outbound notifications — email (SMTP/SendGrid), SMS, Push, WebSocket (SignalR for real-time dashboard updates), in-app notification center
- Fully event-driven for both responsibilities — a single event like `LeaveRequestApproved` triggers both a RequestMetaData status update AND an email notification to the employee, handled by two separate MediatR handlers inside the same service
- Replaces the old Worker project entirely — each business service runs its own Hangfire for scheduling, no centralized HTTP-calling scheduler
- Same Clean Architecture template as business services

---

## IV. RequestMetaData — The Aggregator Table

### What It Is

A single, read-optimized table that holds base information for **every request across all services**. It acts as a unified index for the dashboard, search, and reporting.

### Schema

| Column | Type | Description |
|---|---|---|
| RequestId | GUID (PK) | Unique identifier |
| RequestType | string | e.g., "Leave", "PaymentExecution" |
| RequestCategory | string | e.g., "HR", "Lending", "Payments", "CashOps", "Deposits", "Customer", "Documents", "Compliance" — used for role-based filtering |
| ServiceOrigin | string | Which microservice owns it |
| ExternalRequestId | string | The ID in the owning service's table |
| Status | string | Created / InProgress / Approved / Rejected / Cancelled |
| CreatedDate | datetime | When the request was created |
| CreatedBy | string | User who created it |
| ModifiedDate | datetime | Last modification timestamp |
| ModifiedBy | string | User who last modified it |
| TenantCode | string | BG / RO / GR |
| AssignedTo | string | Current assignee |
| Department | string | Department the request belongs to — used for department-level visibility |
| Priority | int | Request priority level |
| Summary | string | Short human-readable description |

### Authorization — Hybrid Model: Role Policies + User-Level Overrides

The authorization system uses **two layers that work together**:

1. **Role-based policies (the foundation)** — define what a typical person in a given role can do. This covers 95% of users. When you onboard 50 new employees, they all get the "Employee" role and instantly have the correct permissions. When someone is promoted to HRManager, you assign the role and they inherit everything.

2. **User-level overrides (the exceptions)** — grant additional permissions or revoke specific permissions for individual users on top of their role baseline. This covers the real-world 5%: the senior loan officer trusted to approve loans even though LoanOfficer normally can't, or the manager on disciplinary review who needs Approve temporarily revoked.

**Resolution order:** Start with role permissions → apply user-level grants (add) → apply user-level revocations (remove) → final resolved permission set.

The CoreService (and each business service) receives the user's JWT claims (forwarded from the gateway) on every request. The claims contain the user's roles, tenant, department, and userId. **Every operation is checked against the resolved permission set.**

**Two enforcement points:**

- **CoreService** enforces Read-level policies on the dashboard — filtering which requests appear in list/search/stats results.
- **Each business service** enforces Create, Update, Delete, Approve, and Reassign policies on actual operations — the gateway routes the request, but the service checks the resolved permissions before executing the handler.

---

**Action types (the building blocks of every policy):**

| Action | Description | Where Enforced |
|---|---|---|
| Read | Can see this request type in dashboard, search, and detail views | CoreService |
| Create | Can create a new request of this type | Business service (e.g., EmployeeManagementService) |
| Update | Can modify an existing request of this type (edit fields, add documents, change details) | Business service |
| Delete | Can cancel/delete a request of this type | Business service |
| Approve | Can approve or reject a request of this type (step-based workflow progression) | Business service |
| Reassign | Can reassign a request to a different user | Business service |

---

### Layer 1: Role-Based Policies (The Baseline)

Each role defines a default set of permissions per request type. This is the starting point for every user who has that role.

```
{
  "RolePolicies": {
    "HRManager": {
      "tenantScoped": true,
      "departmentScoped": true,
      "personalOnly": false,
      "permissions": {
        "Leave":           ["Read", "Create", "Update", "Approve"],
        "SickLeave":       ["Read", "Create", "Update", "Approve"],
        "BusinessTrip":    ["Read", "Create", "Update", "Approve"],
        "NewEmployee":     ["Read", "Create", "Update"],
        "Complaint":       ["Read", "Create", "Update"],
        "ChangePersonalData": ["Read", "Create", "Update"],
        "ExpenseRequest":  ["Read", "Approve"],
        "Certificate":     ["Read", "Create"],
        "EmployeeState":   ["Read"]
      }
    },
    "HRDirector": {
      "tenantScoped": true,
      "departmentScoped": false,
      "personalOnly": false,
      "permissions": {
        "Leave":           ["Read", "Create", "Update", "Approve", "Delete"],
        "SickLeave":       ["Read", "Create", "Update", "Approve", "Delete"],
        "BusinessTrip":    ["Read", "Create", "Update", "Approve", "Delete"],
        "NewEmployee":     ["Read", "Create", "Update", "Approve", "Delete"],
        "Complaint":       ["Read", "Create", "Update", "Approve", "Reassign"],
        "ChangePersonalData": ["Read", "Create", "Update", "Approve"],
        "ExpenseRequest":  ["Read", "Create", "Update", "Approve"],
        "EmployeeState":   ["Read", "Update", "Approve"],
        "Certificate":     ["Read", "Create", "Approve"],
        "ExtendedWorkingTime": ["Read", "Create", "Update", "Approve"]
      }
    },
    "LoanOfficer": {
      "tenantScoped": true,
      "departmentScoped": true,
      "personalOnly": false,
      "permissions": {
        "LoanApplicationReview": ["Read", "Create", "Update"],
        "FullEarlyPrepayment":   ["Read", "Create"],
        "PartialRepayment":      ["Read", "Create"],
        "LoanRepaymentReporting": ["Read"],
        "RecurringLoanPayment":  ["Read"]
      }
    },
    "LoanManager": {
      "tenantScoped": true,
      "departmentScoped": false,
      "personalOnly": false,
      "permissions": {
        "LoanApplicationReview": ["Read", "Create", "Update", "Approve", "Reassign"],
        "FullEarlyPrepayment":   ["Read", "Create", "Update", "Approve"],
        "PartialRepayment":      ["Read", "Create", "Update", "Approve"],
        "LoanRepaymentReporting": ["Read"],
        "RecurringLoanPayment":  ["Read", "Update"],
        "NeonCreditRepayment":   ["Read", "Create", "Approve"],
        "AutoLoanCertificate":   ["Read", "Create", "Approve"],
        "AnnexZeroPercentageMarkup": ["Read", "Create", "Approve"]
      }
    },
    "CashDeskOperator": {
      "tenantScoped": true,
      "departmentScoped": true,
      "personalOnly": false,
      "permissions": {
        "CashDeskCashIn":       ["Read", "Create"],
        "CashDeskCashOut":      ["Read", "Create"],
        "CashDeskCoinExchange": ["Read", "Create"],
        "CashDeskRevision":     ["Read"]
      }
    },
    "BranchManager": {
      "tenantScoped": true,
      "departmentScoped": true,
      "personalOnly": false,
      "permissions": {
        "Leave":           ["Read", "Approve"],
        "SickLeave":       ["Read", "Approve"],
        "BusinessTrip":    ["Read", "Approve"],
        "ExpenseRequest":  ["Read", "Approve"],
        "PaymentExecution": ["Read", "Approve"],
        "CashDeskRevision": ["Read", "Approve"],
        "CashDeskCashIn":  ["Read"],
        "CashDeskCashOut": ["Read"],
        "NewCustomer":     ["Read"],
        "LoanApplicationReview": ["Read"],
        "Incident":        ["Read", "Create"]
      }
    },
    "ComplianceOfficer": {
      "tenantScoped": true,
      "departmentScoped": false,
      "personalOnly": false,
      "permissions": {
        "RiskEvent":       ["Read", "Create", "Update"],
        "ConflictOfInterest": ["Read", "Create", "Update"],
        "InternalAuditRecommendation": ["Read", "Create", "Update"],
        "InternalAuditAssignment": ["Read", "Create", "Update", "Reassign"],
        "Incident":        ["Read", "Create", "Update"],
        "InsuranceCancellation": ["Read", "Create"]
      }
    },
    "Admin": {
      "tenantScoped": true,
      "departmentScoped": false,
      "personalOnly": false,
      "canSeeAll": true,
      "permissions": "__ALL_ACTIONS__"
    },
    "Employee": {
      "tenantScoped": true,
      "departmentScoped": false,
      "personalOnly": true,
      "permissions": {
        "Leave":           ["Read", "Create"],
        "SickLeave":       ["Read", "Create"],
        "BusinessTrip":    ["Read", "Create"],
        "Complaint":       ["Read", "Create"],
        "ChangePersonalData": ["Read", "Create"],
        "ExpenseRequest":  ["Read", "Create"],
        "Certificate":     ["Read", "Create"],
        "HotDesk":         ["Read", "Create"],
        "HotParking":      ["Read", "Create"],
        "WorkWear":        ["Read", "Create"],
        "PhoneProblem":    ["Read", "Create"],
        "ExtendedWorkingTime": ["Read", "Create"]
      }
    }
  }
}
```

---

### Layer 2: User-Level Overrides (The Exceptions)

Stored in a database table — not in config files, because user overrides change frequently and need to be managed through an admin UI without redeployments.

**Database table: `UserPermissionOverrides`**

| Column | Type | Description |
|---|---|---|
| Id | GUID (PK) | Unique identifier |
| UserId | string | The user this override applies to |
| RequestType | string | e.g., "Leave", "LoanApplicationReview" |
| Action | string | e.g., "Approve", "Update", "Read" |
| OverrideType | string | "Grant" or "Revoke" |
| Reason | string | Why this override was applied (audit trail) |
| GrantedBy | string | Which admin/manager created this override |
| GrantedDate | datetime | When the override was created |
| ExpiresAt | datetime (nullable) | Optional expiry — override auto-deactivates after this date. Null = permanent until manually removed |
| IsActive | bool | Can be deactivated without deleting (soft disable) |

**Examples of real-world overrides:**

| User | RequestType | Action | OverrideType | Reason | ExpiresAt |
|---|---|---|---|---|---|
| ivan.petrov | LoanApplicationReview | Approve | Grant | Senior officer, trusted for loan approvals during Q4 surge | 2026-01-31 |
| maria.georgieva | Leave | Approve | Revoke | Temporary restriction during HR audit investigation | 2025-12-15 |
| dimitar.nikolov | PaymentExecution | Read | Grant | Cross-department project — needs visibility into payments temporarily | 2025-11-30 |
| elena.todorova | CashDeskCashOut | Create | Revoke | Suspended from cash operations pending investigation | null (permanent until cleared) |
| stoyan.ivanov | RiskEvent | Create | Grant | Seconded to compliance team for 3 months | 2026-03-01 |
| all_new_hires_2025 | * | * | — | — | — |

---

### Permission Resolution — How the Two Layers Combine

When a user makes any request, the authorization system resolves their final permissions using this exact order:

**Step 1 — Collect role permissions:**
- Get all roles assigned to the user (a user can have multiple roles)
- UNION all permissions across all roles
- Example: User has roles `Employee` + `HRManager` → gets Employee permissions + HRManager permissions combined

**Step 2 — Apply user-level grants:**
- Query `UserPermissionOverrides` for this userId where `OverrideType = 'Grant'` AND `IsActive = true` AND (`ExpiresAt IS NULL OR ExpiresAt > NOW()`)
- ADD these permissions to the resolved set
- Example: ivan.petrov has LoanOfficer role (no Approve on LoanApplicationReview) + a Grant override for Approve on LoanApplicationReview → he now has Approve

**Step 3 — Apply user-level revocations (revocations always win):**
- Query `UserPermissionOverrides` for this userId where `OverrideType = 'Revoke'` AND `IsActive = true` AND (`ExpiresAt IS NULL OR ExpiresAt > NOW()`)
- REMOVE these permissions from the resolved set, regardless of what roles granted them
- Example: maria.georgieva has HRManager role (Approve on Leave) + a Revoke override for Approve on Leave → she loses Approve on Leave, even though her role gives it
- **Revocations are applied last and always take priority** — this is critical for security. If you need to restrict someone, a Revoke override guarantees they can't do it, no matter how many roles they have.

**Step 4 — Final resolved permission set is used for the request.**

```
Pseudocode:
resolvedPermissions = UNION(all role permissions)
resolvedPermissions = resolvedPermissions + activeGrants
resolvedPermissions = resolvedPermissions - activeRevocations
→ use resolvedPermissions for authorization check
```

---

### Admin UI for Managing Overrides

The system needs a simple admin interface (accessible only to Admin role) for managing user overrides:

- **View overrides by user** — search for a user, see all their active overrides, expiry dates, and who granted them
- **Grant a new permission** — select user, request type, action, reason, optional expiry date
- **Revoke a permission** — select user, request type, action, reason, optional expiry date
- **Deactivate/reactivate an override** — soft toggle without deleting (preserves audit history)
- **Expired overrides** — shown in a separate tab for audit purposes, never hard-deleted
- **Bulk operations** — grant/revoke the same override for multiple users at once (e.g., "all cash desk operators in Varna branch lose CashDeskCashOut Create while we investigate")

**Audit trail:** Every change to `UserPermissionOverrides` is logged — who changed it, when, what the previous state was. This is a banking system; regulators may ask "who gave this person approval rights and when?"

---

### Expiring Overrides — Automatic Cleanup

- Overrides with an `ExpiresAt` date are automatically ignored once expired — the resolution query filters on `ExpiresAt > NOW()`.
- No background job needed to "clean up" — expired overrides simply stop being included in the resolution.
- A scheduled report (weekly or monthly) can flag recently expired overrides for review: "These temporary permissions expired last week — confirm they should stay expired."
- Overrides without an expiry (`ExpiresAt = null`) are permanent until manually deactivated or deleted. These should be reviewed periodically (quarterly audit).

---

### How It Works End-to-End

**Scenario 1 — Normal user, no overrides (95% of cases):**
1. Employee calls `GET /requests` on the dashboard
2. System resolves: roles = ["Employee"] → permissions from role policy → no overrides found
3. Dashboard shows only their own requests (personalOnly = true) with Read permission

**Scenario 2 — User with a grant override:**
1. ivan.petrov (LoanOfficer) calls `POST /api/lending/loan-review/456/approve`
2. System resolves: roles = ["LoanOfficer"] → LoanOfficer has no Approve on LoanApplicationReview
3. Check overrides → finds active Grant for Approve on LoanApplicationReview, not expired
4. Resolved permissions now include Approve → request proceeds, approval executed

**Scenario 3 — User with a revoke override (security restriction):**
1. maria.georgieva (HRManager) calls `POST /api/hr/leave/789/approve`
2. System resolves: roles = ["HRManager"] → HRManager has Approve on Leave
3. Check overrides → finds active Revoke for Approve on Leave, not expired
4. Revoke removes Approve from resolved set → 403 Forbidden returned
5. maria.georgieva can still Read and Create leave requests (those weren't revoked), just not Approve

**Scenario 4 — User with multiple roles + overrides:**
1. dimitar.nikolov has roles ["Employee", "BranchManager"] + Grant override for Read on PaymentExecution
2. Resolved: Employee permissions UNION BranchManager permissions UNION Grant(PaymentExecution.Read)
3. He sees all his own requests (Employee, personalOnly) PLUS branch-scoped requests from BranchManager permissions PLUS payment execution requests from the override

---

### Frontend Integration

On login (or on role/permission change), the frontend calls:

`GET /api/auth/my-permissions`

This returns the user's **fully resolved permission set** (after role union + grants + revocations):

```
{
  "userId": "ivan.petrov",
  "tenant": "BG",
  "department": "Lending - Sofia",
  "departmentScoped": true,
  "personalOnly": false,
  "permissions": {
    "LoanApplicationReview": ["Read", "Create", "Update", "Approve"],
    "FullEarlyPrepayment": ["Read", "Create"],
    "PartialRepayment": ["Read", "Create"],
    "LoanRepaymentReporting": ["Read"],
    "RecurringLoanPayment": ["Read"]
  },
  "overrides": [
    {
      "requestType": "LoanApplicationReview",
      "action": "Approve",
      "type": "Grant",
      "expiresAt": "2026-01-31",
      "reason": "Senior officer, trusted for loan approvals during Q4 surge"
    }
  ]
}
```

The frontend uses this to:
- Show/hide navigation menu items (don't show "Cash Operations" if the user has zero Read permissions in that area)
- Show/hide action buttons (hide "Approve" button if user doesn't have Approve on that request type)
- Show/hide "Create New" options (only show request types where user has Create)
- Display a notice when permissions come from an override (e.g., a small badge: "Temporary permission — expires Jan 31")

**The backend always re-validates** — the UI is a convenience for user experience, not a security boundary. Even if someone manipulates the frontend, the backend will reject unauthorized operations.

---

### Where the Data Lives

| Data | Storage | Why |
|---|---|---|
| Role policies (Layer 1) | `appsettings.json` or shared config store | Roles change rarely. Config is versioned in git. Hot-reload via `IOptionsMonitor` for changes without restart. |
| User overrides (Layer 2) | Database table (`UserPermissionOverrides`) in the CoreService database | Overrides change frequently, need an admin UI, need audit trail, need expiry tracking. Config files are wrong for this — you'd need redeployments for every exception. |
| Resolved permissions cache | In-memory cache (per user, short TTL: 1–5 minutes) | Resolving permissions on every request means role policy lookup + database query for overrides. Cache the resolved set for a few minutes to avoid the DB hit on every call. Invalidate on override change via event. |

---

**Authorization levels (applied cumulatively as AND conditions on Read queries):**

- **Tenant isolation (mandatory, always applied)** — a user in tenant BG never sees requests from RO or GR. The `TenantCode` filter is always applied based on the JWT `tenant` claim. Non-negotiable.

- **Request type + action filtering** — the dashboard only shows request types where the user's resolved permissions include "Read". The user never sees request types outside their resolved policy.

- **Department-level visibility** — if any of the user's roles has `departmentScoped: true`, the user only sees requests from their own department within their allowed request types. Roles like `HRDirector` or `Admin` bypass this. A user-level override can grant Read on a request type but it still respects department scoping from the role — unless the override explicitly includes a `departmentScoped: false` flag.

- **Personal visibility** — if the user's highest-privilege role has `personalOnly: true` (and no other role overrides it), they only see requests where `CreatedBy = currentUserId` or `AssignedTo = currentUserId`. A user with both "Employee" (personalOnly) and "BranchManager" (not personalOnly) gets the BranchManager's broader visibility.

### How It Gets Updated

This table is **READ-ONLY from the dashboard's perspective**. It is **NEVER written to directly by API calls**. It is updated **ONLY by domain events** published by the owning services.

**Flow:**
1. EmployeeManagementService creates a new Leave request → publishes `LeaveRequestCreated` event
2. CoreService consumes the event → inserts a row into RequestMetaData
3. EmployeeManagementService approves the Leave request → publishes `LeaveRequestApproved` event
4. CoreService consumes it → updates the Status column

This is **eventual consistency** — the dashboard might be 1–2 seconds behind the source service. Perfectly acceptable for a dashboard/search use case.

**Event bus:** RabbitMQ with MassTransit (or Azure Service Bus if cloud-hosted). MassTransit handles retries, dead-letter queues, and serialization out of the box on .NET.

### Where It Lives — CoreService

- **NOT in the gateway** — the gateway must be stateless. Adding a database makes it a single point of failure with state.
- **NOT in a business service** — no single service "owns" all request types. Forcing it into EmployeeManagementService or BankingOperationsService creates artificial coupling.
- **Its own dedicated service** with one job: maintain the aggregated view.

### Endpoints

- `GET /requests` — paginated, filterable by status/type/tenant/date range/assignee, sortable. **All results pre-filtered by the user's role and permissions — no unauthorized data ever leaves the service.** Main dashboard endpoint.
- `GET /requests/{id}` — returns metadata + deep-link to the owning service for full details. **Returns 403 Forbidden if the user's role doesn't permit access to that request's category.**
- `GET /requests/stats` — aggregations for dashboard widgets (counts by status, type, tenant, trends). **Stats are scoped to the user's authorized categories and department — an HR Manager sees HR stats, not system-wide totals.**

### Why This Solves the Old Problem

- **Old system:** to show "all requests" you had to query 90+ entity tables, UNION them, sort, paginate — slow, lock-prone, impossible to optimize.
- **New system:** one SELECT on one pre-aggregated table with proper indexes. Sub-millisecond response times. No cross-service queries. No database locks on source tables.
- **Pagination:** keyset pagination on `CreatedDate + RequestId` for consistent, fast paging even with millions of rows.

---

## V. API Gateway — YARP on .NET 10

### Why YARP

- **YARP (Yet Another Reverse Proxy)** is Microsoft's high-performance, .NET-native reverse proxy. Runs inside ASP.NET, integrates with the existing middleware pipeline — auth, logging, rate limiting, everything.

- No external dependency like Kong, Ocelot (unmaintained), or NGINX. Same .NET 10 tooling, same deployment model, same team can maintain it.

- **Configuration-driven routing** — routes defined in `appsettings.json` or loaded dynamically. No code changes to add/remove routes.

- Supports load balancing, health checks, session affinity, header/path transforms, and rate limiting out of the box.

### What the Gateway Does

- **Authentication routing** — login and token refresh calls (`POST /api/auth/login`, `POST /api/auth/refresh`) are routed to **EmployeeManagementService**, which is the single source of truth for employee identity. The gateway does not handle login itself — it just routes the request.

- **JWT validation** — for all non-auth requests, the gateway validates the JWT signature using the shared signing key. Valid tokens have their claims forwarded to downstream services as headers. No downstream service re-validates tokens — they trust the forwarded claims. Invalid or expired tokens are rejected at the gateway with 401 before reaching any service.

- **Coarse-grained authorization** — "is this user allowed to call `/api/banking/*`?" at the gateway. Fine-grained authorization ("can this user approve THIS loan?") stays in the service.

- **Tenant resolution** — extract tenant from JWT claims, inject `X-Tenant-Id` header for downstream services.

- **API versioning** — `/v1/employee/leave` routes to EmployeeManagementService v1, `/v2/employee/leave` routes to v2. Versioning is gateway configuration.

- **Correlation ID** — generate `X-Correlation-Id` for every request, propagate to all downstream calls and logs.

- **Rate limiting** — per-tenant, per-endpoint using ASP.NET's `RateLimiter` middleware.

- **Request/response logging** — method, path, status code, duration. Feed into observability stack.

### What the Gateway Does NOT Do

- **No business logic** — the gateway routes, it doesn't decide.
- **No database** — stateless. Crash and restart with zero data loss.
- **No payload transformation** — doesn't reshape request/response bodies.
- **No service-to-service communication** — services talk via events (MassTransit/RabbitMQ), not through the gateway.
- **No token issuance** — JWTs are issued by EmployeeManagementService, not the gateway.

---

## VI. CoreService — Orchestration + Communication

CoreService is the combined infrastructure service that replaced both the RequestOrchestratorService and the CommunicationService. It has no business domain of its own — it is a pure event consumer that reacts to what the 4 business services publish.

### Two Responsibilities, One Service

**Responsibility 1 — Request Orchestration:**
- Consumes domain events from all business services and maintains the RequestMetaData table
- Exposes the dashboard, search, and stats endpoints with full role/policy-based authorization
- Owns the UserPermissionOverrides table — user-level permission overrides are stored here and resolved at query time

**Responsibility 2 — Notifications:**
- Consumes the same domain events and delivers notifications through the appropriate channel
- Channels: Email (SMTP/SendGrid), SMS (Twilio/provider), Push Notifications, WebSocket (SignalR for real-time dashboard updates), In-app notification center
- A single event triggers both responsibilities — `LeaveRequestApproved` causes CoreService to update the RequestMetaData status AND send an email to the employee, handled by two separate MediatR handlers inside the same service

**Event examples:**

| Event | Orchestration reaction | Notification reaction |
|---|---|---|
| `LeaveRequestApproved` | Update RequestMetaData status → Approved | Email to employee + WebSocket dashboard update |
| `PaymentExecutionFailed` | Update RequestMetaData status → Failed | SMS to operations team + in-app alert |
| `LoanApplicationSubmitted` | Insert new RequestMetaData row | Email confirmation to customer + notification to reviewer |
| `NewEmployeeCreated` | Insert new RequestMetaData row | Welcome email to employee |

### Replacing the Old Worker

The old Worker project was a scheduled job runner that called the monolith's API via HTTP to trigger notifications and background tasks — tight coupling, HTTP overhead, single point of failure.

- CoreService is fully event-driven. No HTTP calls to other services.
- **Scheduling:** each business service runs **Hangfire internally** for its own scheduled tasks. No centralized scheduler.
- **Outbox Pattern:** when a business service publishes an event, it writes the event to an outbox table in the **same transaction** as the business data. A background process reads the outbox and publishes to RabbitMQ. Guarantees at-least-once delivery even if RabbitMQ is temporarily down.

---

## VII. Observability & Reliability

### Observability Stack

- **OpenTelemetry** for distributed tracing — every request gets a trace spanning gateway → service → database → event publication. Visualize with Jaeger or Zipkin.

- **Structured logging with Serilog** — every log line is JSON with CorrelationId, ServiceName, TenantCode, UserId. Ship to Seq or ELK stack.

- **Metrics with Prometheus + Grafana** — request rate, error rate, response time (P50/P95/P99), DB connection pool usage, thread pool queue length, message queue depth. One dashboard per service.

- **Health checks** — every service exposes `/health` (liveness) and `/ready` (readiness). Gateway stops routing to unhealthy instances.

### Resilience Patterns

- **Circuit Breaker (Polly)** — stop calling a failing downstream for a cooldown period. Prevents cascade failures.

- **Retry with exponential backoff** — transient failures get retried with increasing delays. Max 3 retries, then fail gracefully.

- **Timeout policies** — every HTTP call and database query has an explicit timeout.

- **Bulkhead isolation** — limit concurrent calls to each downstream dependency.

- **Graceful degradation** — if CoreService is down, business requests still succeed. Notifications queued in outbox and delivered on recovery.

### Deployment & High Availability

- **Minimum 2 instances per service** behind a load balancer.
- **Rolling deployments** — deploy one instance at a time, auto-rollback on failed health checks.
- **Database per service** — if BankingOperationsService DB has issues, EmployeeManagementService is unaffected.
- **Database replication** — read replicas for heavy-read services (CoreService, reporting).
- **Gateway redundancy** — at least 2 YARP instances behind a load balancer.

---

## VIII. Implementation Phases

### Phase 1 — Foundation (Weeks 1–4)

- Set up solution structure: gateway project, shared NuGet packages (`Result<T>`, base entities, event contracts, MediatR pipeline behaviors for validation/logging), service project template based on Clean Architecture.
- Configure CI/CD pipeline with test gates — no merge without passing tests.
- Set up YARP gateway with authentication, tenant resolution, correlation ID, rate limiting.
- Set up RabbitMQ + MassTransit for event-driven communication.
- Set up observability: OpenTelemetry, Serilog, Prometheus, Grafana dashboards.
- Create CoreService with RequestMetaData table and dashboard endpoints.

### Phase 2 — First Service Extraction (Weeks 5–10)

- Pick the highest-pain domain first (likely BankingOperationsService or EmployeeManagementService — whichever causes the most performance issues).
- Build from scratch using the Clean Architecture template. MediatR handlers for every use case. FluentValidation on every request. Full test coverage. Proper transactions. Projection-based queries.
- Integrate with gateway and CoreService.
- Load test. Establish performance baselines.
- Deploy alongside old system. Feature flags or gateway routing for gradual traffic shift.

### Phase 3 — Parallel Extraction (Weeks 11–24)

- With the template proven, extract 2–3 services in parallel.
- Each follows: define scope → scaffold from template → build handlers → write tests → integrate → load test → deploy.
- **CoreService built early** since many services depend on it for notifications.

### Phase 4 — Complete Migration & Decommission (Weeks 25+)

- Extract remaining services. Monolith shrinks with each extraction.
- Decommission monolith once all request types are served by microservices.
- Final performance benchmarking against old system metrics.
- Architecture documentation, runbooks per service, onboarding guides.

---

## Appendix: Full Request Type Mapping (85 types → 6 business services)

| # | Request Type | Service |
|---|---|---|
| 1 | NewEmployee | EmployeeManagementService |
| 2 | Complaint | EmployeeManagementService |
| 3 | Leave | EmployeeManagementService |
| 4 | BusinessTrip | EmployeeManagementService |
| 5 | Change | EmployeeManagementService |
| 6 | Vehicle | EmployeeManagementService |
| 7 | Stamp | EmployeeManagementService |
| 8 | BusinessCard | EmployeeManagementService |
| 9 | WorkWear | EmployeeManagementService |
| 10 | HotDesk | EmployeeManagementService |
| 11 | ChangePersonalData | EmployeeManagementService |
| 12 | HotParking | EmployeeManagementService |
| 13 | SickLeave | EmployeeManagementService |
| 14 | Covid | EmployeeManagementService |
| 15 | Certificate | EmployeeManagementService |
| 16 | IdentificationDeclaration | EmployeeManagementService |
| 17 | PhoneProblem | EmployeeManagementService |
| 18 | ExtendedWorkingTime | EmployeeManagementService |
| 19 | EmployeeState | EmployeeManagementService |
| 20 | ExpenseRequest | EmployeeManagementService |
| 21 | FullEarlyPrepayment | BankingOperationsService |
| 22 | PartialRepayment | BankingOperationsService |
| 23 | LoanApplicationReview | BankingOperationsService |
| 24 | LoanRepaymentReporting | BankingOperationsService |
| 25 | RecurringLoanPayment | BankingOperationsService |
| 26 | ThirdPartyLoanInstallmentPayment | BankingOperationsService |
| 27 | NeonCreditRepayment | BankingOperationsService |
| 28 | AutoLoanCertificate | BankingOperationsService |
| 29 | AnnexZeroPercentageMarkup | BankingOperationsService |
| 30 | PaymentExecution | BankingOperationsService |
| 31 | DirectDebit | BankingOperationsService |
| 32 | UnrecognizedPayment | BankingOperationsService |
| 33 | PaymentAccountOpening | BankingOperationsService |
| 34 | RefundOverpaid | BankingOperationsService |
| 35 | SWIFT | BankingOperationsService |
| 36 | CashDeskRevision | BankingOperationsService |
| 37 | CashDeskCoinExchange | BankingOperationsService |
| 38 | CashDeskCashIn | BankingOperationsService |
| 39 | CashDeskCashOut | BankingOperationsService |
| 40 | OperationalDeskCashIn | BankingOperationsService |
| 41 | OperationalDeskCashOut | BankingOperationsService |
| 42 | MainOperationalDeskTransfer | BankingOperationsService |
| 43 | InkassoCashIn | BankingOperationsService |
| 44 | InkassoCashOut | BankingOperationsService |
| 45 | DepositOpening | BankingOperationsService |
| 46 | RaisinDepositOpening | BankingOperationsService |
| 47 | AccountClosing | BankingOperationsService |
| 48 | Check24Deposit | BankingOperationsService |
| 49 | NewCustomer | BankingOperationsService |
| 50 | NewExternalCustomer | BankingOperationsService |
| 51 | Leads | BankingOperationsService |
| 52 | ChangeCustomerSector | BankingOperationsService |
| 53 | ChangeCustomerTransactionalLimits | BankingOperationsService |
| 54 | DeceasedCustomer | BankingOperationsService |
| 55 | OnlineBanking | BankingOperationsService |
| 56 | ResetStaticPassword | BankingOperationsService |
| 57 | ResetDigitalBankingPassword | BankingOperationsService |
| 58 | AccessAndAuthentication | BankingOperationsService |
| 59 | DebitCardIssuing | BankingOperationsService |
| 60 | CardCurrentStatus | BankingOperationsService |
| 61 | DigitalSigning | BankingOperationsService |
| 62 | SmallMerchantRegistration | BankingOperationsService |
| 63 | MerchantAnnex | BankingOperationsService |
| 64 | ActivateDeactivateSchemes | BankingOperationsService |
| 65 | IncomingCorrespondence | BankingOperationsService |
| 66 | OutgoingCorrespondence | BankingOperationsService |
| 67 | IncomingCorrespondenceAutomation | BankingOperationsService |
| 68 | OutgoingCorrespondenceAutomation | BankingOperationsService |
| 69 | IncomingCorrespondenceAuditConfirmation | BankingOperationsService |
| 70 | AnswersToInstitutions | BankingOperationsService |
| 71 | AnswersToCustomers | BankingOperationsService |
| 72 | ContractCopy | BankingOperationsService |
| 73 | ContractCancellation | BankingOperationsService |
| 74 | PowerOfAttorney | BankingOperationsService |
| 75 | BankReference | BankingOperationsService |
| 76 | RiskEvent | ComplianceService |
| 77 | ConflictOfInterest | ComplianceService |
| 78 | InternalAuditRecommendation | ComplianceService |
| 79 | InternalAuditAssignment | ComplianceService |
| 80 | InsuranceCancellation | ComplianceService |
| 81 | Incident | ComplianceService |
| 82 | Treasury | ComplianceService |
| 83 | MRelTreasury | ComplianceService |
| 84 | Navigator | ComplianceService |
| 85 | Request | CoreService |

### Service Distribution Summary

| Service | Request Count | Sub-Domains |
|---|---|---|
| EmployeeManagementService | 20 | Employee lifecycle, workplace logistics |
| BankingOperationsService | 55 | Payments, cash ops, deposits, lending, customer data, cards, merchants, documents |
| ComplianceService | 9 | Risk, audit, integrations |
| CoreService | 1 (RequestMetaData) | Dashboard, notifications, event orchestration, authorization |

**Total: 3 business services + 1 CoreService + 1 API gateway**

---

*Architecture Plan v7.0 — .NET 10 + YARP + Clean Architecture + MediatR + Event-Driven*
