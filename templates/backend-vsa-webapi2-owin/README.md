[English](README.md) | [简体中文](README.zh-CN.md)

# Backend VSA: Web API 2 and OWIN

A runnable .NET Framework 4.8 reference template using ASP.NET Web API 2, a Katana/OWIN pipeline, and a console process self-hosted on `HttpListener`.

## Prerequisites

- Windows with the .NET Framework 4.8 Developer Pack
- .NET SDK 10 or later, capable of building SDK-style `net48` projects

The local `NuGet.Config` intentionally uses only nuget.org so restore behavior does not depend on machine-level package sources.

## Architecture

This README is the authoritative source for this template's concrete project and directory layout, runtime instructions, and stack-specific details.

```text
src/
├── BuildingBlocks/                                  # stable, reusable, business-agnostic architectural foundations
│   ├── BackendVsaOwin.BuildingBlocks.WebApi/        # Web API 2 transport primitives
│   └── BackendVsaOwin.BuildingBlocks.Persistence/   # shared SQLite connection and migration infrastructure
├── Host/                                            # executable hosts and composition roots
│   └── BackendVsaOwin.Host/                         # executable host and composition root
└── Modules/                                         # business modules and vertical slices
    ├── BackendVsaOwin.Modules.Customers.Contracts/  # smallest public Customers contract
    ├── BackendVsaOwin.Modules.Customers/            # Customers business module and vertical slices
    └── BackendVsaOwin.Modules.Orders/               # Orders business module and vertical slices

tests/                                               # unit tests and host integration tests
├── BackendVsaOwin.Modules.Customers.Tests/          # Customers module tests
├── BackendVsaOwin.Modules.Orders.Tests/             # Orders module tests
└── BackendVsaOwin.Host.IntegrationTests/            # HTTP, pipeline, and OpenAPI integration tests
```

The executable Host is organized by responsibility:

```text
BackendVsaOwin.Host/
├── Authentication/   # Basic/OAuth authentication and challenge integration
├── Composition/      # application identity and Host-owned module descriptors
├── OpenApi/          # NSwag configuration and document processors
├── Persistence/      # database path configuration
├── WebApi/           # Web API configuration, discovery, DI, and exception handling
├── App.config
├── Program.cs
└── Startup.cs
```

The Host owns `WebApp.Start`, the OWIN pipeline, the Microsoft DI container, Web API configuration, NSwag, database migration orchestration, and module composition. Its explicit, Host-owned module descriptors define each module's assembly, service-registration delegate, and dependencies in one ordered catalog. The catalog derives the runtime Controller whitelist and migration order and invokes module service registration, so an unlisted assembly cannot add an endpoint or migration accidentally. No reflection-based module discovery is used. Each business module owns its HTTP actions, request and response models, validation, handlers, domain objects, SQLite Store, and embedded migration scripts. `BackendVsaOwin.BuildingBlocks.WebApi` contains only shared HTTP transport primitives, including RFC 9457 error contracts; it does not contain domain results or business rules. `BackendVsaOwin.BuildingBlocks.Persistence` contains the reusable SQLite connection factory and DbUp runner; module-specific SQL and Stores remain inside their owning module. The custom Web API dependency resolver creates and disposes one `IServiceScope` per request.

Orders references only `Customers.Contracts` and resolves customers through `ICustomerLookup`; it cannot access Customers domain or persistence types. Customers implements that contract, while Host connects the implementation at startup. An order stores `CustomerId` plus a customer-name snapshot so historical order data does not change when the current customer name changes.

```text
Host -> Customers -> Customers.Contracts
Host -> Orders    -> Customers.Contracts
Host, Customers, Orders -> BuildingBlocks.WebApi
Host, Customers, Orders -> BuildingBlocks.Persistence
Orders -X-> Customers internals
```

Each use case remains in its own feature directory. Web API 2 selects a controller before it evaluates action-level HTTP method constraints, so actions that share `/api/orders` must belong to one controller type. The slice action files therefore compose one `partial OrdersController`; this is a transport-layer compatibility choice, not a shared business-service layer.

The template fixes the language version at C# 14. Pure output DTOs use `required init` properties and named object initializers, while request DTOs retain setters and explicit validators and domain objects retain constructors or factories for invariants. PolySharp supplies the compiler-support types that features such as `required` and `init` need on `net48`; it remains a private build-time dependency and does not add a deployed runtime assembly.

CLR types and properties follow .NET PascalCase conventions, while the HTTP JSON contract uses camelCase for request, response, and error properties. Web API's Newtonsoft formatter owns this naming policy, and NSwag reuses the same serializer settings so runtime payloads and OpenAPI schemas cannot drift. API errors use RFC 9457 Problem Details with the `application/problem+json` media type. The standard `type` URI is the only machine-readable problem identifier; no parallel `code` property is emitted. The permitted `traceId` extension identifies one request occurrence and matches the `X-Trace-Id` response header and structured server log.

The global `ModelStateValidationFilter` runs after Web API parameter binding and before an action executes. It converts binding and type-conversion failures into the shared validation Problem Details response without exposing formatter exception details. Feature validators run only after transport binding succeeds; they own use-case rules such as positive totals, batch limits, and cross-module reference checks. A missing body is handled at the controller boundary, so feature validators receive non-null request objects and remain independent of Web API `ModelState` types.

All three test projects use the xUnit.net v3 programming model on Microsoft Testing Platform, keeping the same test style available to future modern .NET templates. The template-level `global.json` selects the MTP runner for `dotnet test` on .NET SDK 10 and later.

## Development

Run these commands from this directory:

```powershell
dotnet restore BackendVsaOwin.sln
dotnet build BackendVsaOwin.sln --no-restore --configuration Release
dotnet test BackendVsaOwin.sln --no-build --configuration Release
dotnet run --project src/Host/BackendVsaOwin.Host -- http://localhost:5088/
```

Stop the host with `Ctrl+C`. A different URL can be supplied as the first command-line argument; the default is also stored in `src/Host/BackendVsaOwin.Host/App.config`.

## Persistence

Customers and Orders use one SQLite database. The default relative path is resolved from the Host executable directory and can be changed in `App.config`:

```xml
<add key="DatabasePath" value="data/backend-vsa-owin.db" />
```

The Host creates the parent directory and application logging pipeline, enables and verifies persistent SQLite WAL mode, and then runs embedded DbUp migrations before accepting traffic. DbUp sends migration discovery, execution, and no-op diagnostics through the same JSON logging provider used by the application. The module catalog validates that every dependency has been declared earlier, then migrations run deterministically in that order: Customers first, then Orders. Both modules share the database's default `SchemaVersions` journal, while each module owns its own numbered SQL scripts under `Migrations/`. Applied scripts are tracked by resource name and must not be edited; add a new numbered script for every schema change.

Each Store operation opens and disposes its own connection. `Foreign Keys=True` and an explicit 30-second lock-wait timeout are applied to every connection; deployments with stricter latency requirements can override the timeout when constructing the connection factory. Module Stores use Dapper only for parameterized runtime SQL and row materialization. Their private persistence rows keep SQLite GUID values as canonical strings for explicit domain conversion while mapping amount `TEXT` directly to `decimal`; immutable domain objects remain independent of Dapper. `orders.customer_id` references `customers.id` with `ON DELETE RESTRICT` and `ON UPDATE RESTRICT`; application validation through `ICustomerLookup` remains the user-facing check, and the foreign key is the final integrity guard. Microsoft.Data.Sqlite stores order `decimal` values as `TEXT`, preserving their complete precision without SQLite `REAL` round-trip loss. Batch creation and deletion execute in short, explicitly controlled database transactions.

## HTTP Endpoints

| Method | Path | Purpose |
| --- | --- | --- |
| `POST` | `/api/customers` | Validate and create a customer. |
| `GET` | `/api/customers/{id}` | Get one customer by ID. |
| `POST` | `/api/orders` | Validate a customer reference and create an order. |
| `POST` | `/api/orders/batch` | Validate customer references and atomically create a batch of orders. |
| `GET` | `/api/orders` | List all orders in deterministic ID order. |
| `GET` | `/api/orders/{id}` | Get one order by ID. |
| `PUT` | `/api/orders/{id}` | Validate and replace the order's mutable fields. |
| `DELETE` | `/api/orders/{id}` | Delete one order. |
| `POST` | `/api/orders/batch-delete` | Validate and delete a batch of orders. |
| `POST` | `/oauth/token` | Issue a demo OAuth2 bearer token from resource-owner credentials. |
| `GET` | `/swagger/v1/swagger.json` | Return the generated OpenAPI 3.0 document. |
| `GET` | `/swagger` | Open Swagger UI. |

## Authentication

The template supports two alternative authentication schemes for the same protected Web API operations: Katana Basic authentication and OAuth2 bearer authentication. Swagger UI and its OpenAPI document are registered before the authentication middleware and remain public; `/oauth/token` is handled by the OAuth authorization-server middleware and remains available without Basic authentication. A global Web API `AuthorizeAttribute` requires an authenticated identity for every API endpoint. The generated OpenAPI document declares both schemes as separate security requirements, which means Basic OR OAuth2, so Swagger UI displays lock icons for either scheme and is configured with the application name for its OAuth2 client settings.

`BasicAuthenticationHandler` parses the HTTP header, creates the Katana authentication ticket, and applies the Basic challenge when the Basic scheme is selected. `PasswordGrantOAuthProvider` uses the same `ICredentialValidator` for the OAuth2 resource-owner password grant and creates an opaque bearer token. The default `ConfiguredCredentialValidator` performs fixed-time comparisons against the single credential loaded from `App.config`, so another credential source can replace it without changing either transport component. `BasicAuthenticationOptions` contains only the Basic scheme metadata and Realm, not credentials.

The demo credential is configured in `src/Host/BackendVsaOwin.Host/App.config`:

```xml
<add key="Username" value="admin" />
<add key="Password" value="password" />
```

`ApplicationIdentity` defines the compile-time `ApplicationName`, `OpenApiTitle`, and `BasicRealm` constants. Change those constants when cloning the template for another application; they are intentionally identical in every environment. `Username` and `Password` remain runtime settings shared by the Basic and OAuth2 demonstration flows.

Send credentials with the standard header (for example, `admin:password` encoded as Base64):

```text
Authorization: Basic YWRtaW46cGFzc3dvcmQ=
```

Missing or invalid credentials leave the request anonymous; the global Web API authorization filter returns HTTP 401, and `SchemeAwareAuthenticationFilter` selects the challenge from the incoming credentials. A request with neither an `Authorization` header nor an `access_token` query parameter advertises both Basic and Bearer. A Basic request advertises only Basic, while an invalid Bearer header or non-empty query token advertises only Bearer. Authentication failures are not application Problem Details responses. The integration test project uses its own `test-user` / `test-password` configuration.

Request a demo bearer token with the password grant. This template intentionally treats the client as public and requires the configured resource-owner credentials:

```http
POST /oauth/token
Content-Type: application/x-www-form-urlencoded

grant_type=password&username=admin&password=password
```

Use the returned `access_token` on the same API endpoints that accept Basic:

```text
Authorization: Bearer <access_token>
```

For constrained clients or protocol handshakes that cannot set an `Authorization` header, the bearer middleware also accepts a query-string token:

```http
GET /api/orders?access_token=<access_token>
```

The `Authorization` header takes precedence when both forms are present, including when the header contains an invalid bearer token; the query token is not used as a fallback. Query-string tokens can be captured by server and proxy logs, browser history, monitoring telemetry, and `Referer` headers. Use this compatibility path only over HTTPS, redact `access_token` from logs and telemetry, and prefer the bearer header for ordinary HTTP clients.

`Microsoft.Owin.Security.OAuth` uses Katana's default protected ticket format for this single-process template. The token endpoint, password grant, and shared demonstration credential are not a production identity system; use HTTPS, confidential client authentication, a real user store, token key management, rotation, and a grant suitable for the deployment threat model.

HTTP and plaintext credentials are intentional for this local self-hosted demonstration. A deployed service must use HTTPS, secret storage, credential rotation, and an authentication scheme appropriate for its threat model; do not keep the sample password.

Create a customer first:

```json
{
  "displayName": "Ada Lovelace"
}
```

Then create an order using the returned customer ID:

```json
{
  "customerId": "9fc75e91-bc60-469d-98a6-d677e6760cd9",
  "totalAmount": 42.50
}
```

Successful creation returns HTTP 201 and a `Location` header for the new resource. Orders resolve the current customer name through the public Customers contract and retain it as `customerName`; callers cannot submit the snapshot directly. An unknown `customerId` returns HTTP 400 before an order is stored. `PUT` changes only `totalAmount`, preserving customer identity and the original name snapshot. Validation failures return HTTP 400 with Problem Details type `urn:backend-vsa-owin:problem:validation-failed` and an `errors` extension keyed by JSON field path. Looking up, updating, or deleting an unknown order ID returns HTTP 404 with type `urn:backend-vsa-owin:problem:order-not-found`. Customer lookup failures use `urn:backend-vsa-owin:problem:customer-not-found`. Each problem includes `type`, `title`, `status`, `detail`, `instance`, and `traceId`; unexpected exceptions become safe HTTP 500 responses while the Host logs the complete exception with the same trace identifier. A valid incoming W3C `traceparent` is continued, otherwise the server starts a new trace. A successful delete returns HTTP 204.

Batch creation accepts between 1 and 100 orders:

```json
{
  "orders": [
    {
      "customerId": "9fc75e91-bc60-469d-98a6-d677e6760cd9",
      "totalAmount": 42.50
    },
    {
      "customerId": "d7231233-8af5-4e3c-ad61-38a78015bfec",
      "totalAmount": 99.95
    }
  ]
}
```

All items and customer references are validated before storage. Customer IDs are resolved in one batch contract call. An invalid or unknown customer returns HTTP 400 with indexed fields such as `orders[1].customerId`, and no orders are written. A valid batch is committed atomically and returns HTTP 201 with `createdCount` and `items`. Because the response represents multiple resources, it does not provide one `Location` header.

Batch deletion accepts between 1 and 100 distinct, non-empty order IDs:

```json
{
  "ids": [
    "b9c359d4-6e94-41ca-aa3c-0af90409d2a1",
    "51c67b21-883e-47cb-8b5f-bc5a7fc23067"
  ]
}
```

A valid batch returns HTTP 200 with `requestedCount`, `deletedCount`, and `missingIds`. Missing IDs do not fail the request or roll back orders already deleted. The command uses POST because DELETE request bodies have inconsistent support across Web API 2 clients and intermediaries.

## Dependency Decisions

- Web API Core 5.3.0 intentionally resolves Web API Client 6.0.0.
- `Microsoft.Owin.Hosting` and `Microsoft.Owin.Host.HttpListener` provide console self-hosting; this is not an IIS project.
- `Microsoft.Owin.Security` and `Microsoft.Owin.Security.OAuth` supply the native Katana Basic and OAuth2 authentication options, middleware, per-request handlers, tickets, and challenge lifecycle. `SchemeAwareAuthenticationFilter` selects the OWIN challenge for the incoming scheme; Web API's global `AuthorizeAttribute` separately enforces authentication.
- The Host replaces Web API's default assembly resolver with an explicit Customers/Orders whitelist; integration tests use a separate test startup to add their faulting Controller without exposing the test assembly in production.
- NSwag serves the API description and UI. Its Newtonsoft schema generator shares the Web API formatter settings so documented property names match runtime JSON. XML documentation and explicit Web API response metadata enrich operation, schema, and response contracts; Controller summaries are also used as OpenAPI tag descriptions. The document post-processor publishes a relative `/` server URL so the document follows the current host and proxy environment. Document processors declare the Basic and OAuth2 security schemes and add the OWIN-handled `/oauth/token` operation under the public `Authentication` tag; the token operation documents OAuth form fields and standard JSON errors and is excluded from the Problem Details media-type rewrite. A global operation processor adds the shared `500` `ProblemDetailsResponse` contract to every generated Web API operation unless that response is explicitly declared; the OWIN token operation intentionally keeps its OAuth error contract. The static-file dependency remains transitive.
- The small `BackendVsaOwin.BuildingBlocks.WebApi` project adapts RFC 9457 Problem Details to Web API 2. It avoids coupling this .NET Framework template to ASP.NET Core MVC packages while keeping error serialization and media types consistent across modules.
- `ProblemTypeUris` centralizes the `urn:backend-vsa-owin:problem` namespace and shared validation type. Module-specific types such as order-not-found and customer-not-found remain owned by their modules; the namespace uses the existing colon-delimited contract format.
- Its global `ModelStateValidationFilter` handles transport binding errors before actions run; feature validators remain responsible for use-case rules and do not depend on Web API `ModelState`.
- `BackendVsaOwin.BuildingBlocks.Persistence` owns only reusable SQLite connection creation and DbUp orchestration. Customers and Orders retain ownership of their Store implementations and embedded SQL migrations.
- `Microsoft.Data.Sqlite` provides the SQLite ADO.NET provider, while Dapper removes command and reader boilerplate inside module Stores without owning connections, transactions, migrations, or domain models. DbUp runs forward-only embedded scripts, records them in one shared `SchemaVersions` journal, and writes migration events through the Host's existing Microsoft Extensions Logging provider. The Host explicitly orders module migrations so Orders can add its foreign key after Customers creates the referenced table.
- `System.Diagnostics.DiagnosticSource` establishes W3C request activities on .NET Framework 4.8. Microsoft Extensions Logging writes JSON-structured exception records to the console; production deployments can replace the provider without changing the exception boundary.
- Microsoft DI 10.0.10 is the container; compatibility support packages remain transitive.
- PolySharp 1.13.0 privately supplies internal compiler polyfills for modern C# syntax on `net48`; generated types are not made public, and `required` does not replace HTTP request validation.
- xUnit.net v3 on Microsoft Testing Platform covers focused slice tests and OWIN TestServer integration tests with isolated temporary SQLite files, without a separate test adapter.
- `AddManyAsync` owns the atomic batch-create boundary and the SQLite implementation enforces it with a database transaction.
- `ICustomerLookup` is the deliberately small cross-module API; Orders does not share repositories, entities, or an HTTP loopback with Customers.
- The custom Basic and OAuth2 schemes deliberately share one credential from `App.config`; `ICredentialValidator` separates credential verification from Katana request processing without introducing a user-store abstraction. Swagger and `/oauth/token` remain public, while downstream Web API requests accept Basic OR Bearer. The password grant, user stores, client authentication, token rotation, and role or policy authorization remain outside this minimal template.
- `ApplicationIdentity` centralizes the compile-time application name, OpenAPI title, and Basic authentication Realm so a cloned template has one application-identity source. Assembly names and namespaces remain compile-time project identity and are not runtime settings.

## Current Limits

The single SQLite file provides durable local persistence, not replication, high availability, or multi-process write coordination. Batch deletion still treats missing IDs as a successful best-effort outcome, although its writes are committed in one transaction. Customer updates and deletes are intentionally absent; the database currently protects referenced customers with `RESTRICT`, so richer referential lifecycle policies remain outside this minimal template. Persistent log aggregation, distributed-trace export, backup automation, and repository-wide automation are also out of scope; the database file must be backed up and protected by the deployment environment. The shared Basic credential and OAuth password grant are demonstration-only authentication boundaries, not a production identity system.
