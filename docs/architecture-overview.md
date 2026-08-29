---
state: Current
last_updated: "2026-08-29"
---

[English](architecture-overview.md) | [简体中文](architecture-overview.zh-CN.md)

# Architecture Overview

## Context and Drivers

This repository teaches and demonstrates how Vertical Slice Architecture (VSA) and Feature-Sliced Design (FSD) can coexist in full-stack systems. It separates reusable guidance from minimal stack-specific starters so that each template remains focused and independently adoptable.

## Repository Boundaries

- `docs/` is the canonical source for rules shared by all stacks.
- Each completed directory under `templates/` is an independent starter and must document its own runtime and commands.
- Templates may reference the documentation, but they must not depend on runtime code from another template.
- Stack-specific commands, configuration, and troubleshooting stay with their owning project.

## Components and Responsibilities

| Component | Responsibility |
| --- | --- |
| `docs/` | Own cross-stack architecture principles, dependency rules, and verification strategy. |
| `templates/backend-*` | Demonstrate VSA and modular-monolith boundaries in one backend stack. The Web API 2/OWIN template is currently implemented. |
| `templates/frontend-*` | Demonstrate FSD dependency direction and public APIs in one frontend stack. The Vue 3 template is currently implemented. |
| `scripts/` | Reserved for future repository-wide automation. |

The concrete project layout and runtime instructions for the implemented Web API 2/OWIN template are maintained exclusively in the [template README](../templates/backend-vsa-webapi2-owin/README.md).

## Dependency Rules

- Backend behavior is organized by use case inside a business module. Detailed backend rules belong to the [VSA Guide](vsa-guide.md).
- Backend modules depend on another module only through a deliberately small public contract; the composition root connects contracts to implementations, and consumers cannot reference another module's domain or persistence internals.
- Frontend imports follow the FSD layer direction and cross slice boundaries through public APIs. Detailed frontend rules belong to [FSD Principles](fsd-principles.md).
- Backend persistence models and domain internals are never frontend contracts.
- Generated API clients, when used, are derived from a published schema and are not hand-edited.
- Authentication, validation, errors, pagination, and versioning require explicit wire-level conventions.

## Frontend and Backend Alignment

VSA and FSD align through user-visible capabilities and HTTP contracts, not through identical folder names or forced one-to-one slices.

```text
FSD page-local workflow
  -> Shared API contract
    -> HTTP API contract
      -> one VSA endpoint and use-case slice
```

- A page-local action or an extracted frontend Feature may map to one backend slice.
- A page may own several workflows and call several backend slices; extraction is based on confirmed frontend reuse, not backend folder symmetry.
- A backend slice may serve multiple frontend entry points when its contract and authorization rules are the same.
- Read models should be shaped for the use case rather than exposing persistence entities.

## Modularity and FSD

Modularity is the architectural goal of keeping related behavior cohesive, dependencies explicit, and changes localized. Feature-Sliced Design is a concrete frontend methodology that supports that goal through optional standardized layers, business-oriented slices, purpose-oriented segments, and public APIs. They are complementary rather than competing alternatives: this repository uses FSD to organize the Vue frontend while using VSA and modular-monolith boundaries in the backend.

The community article [Modular Design vs Feature-Sliced Design in Vue 3](https://dev.to/igornosatov_15/slicing-through-complexity-modular-design-vs-feature-sliced-design-in-vue-3-13dh) provides a useful comparison of these ideas. It is supplementary reading, not a normative source for this repository; the rules in [FSD Principles](fsd-principles.md) and the concrete template README remain authoritative.

## Interfaces and Data

Frontend and backend projects communicate through explicit HTTP API contracts. The backend owns its published schema and compatibility policy. The frontend consumes that contract without importing backend implementation code. Plain CRUD functions and wire types may remain in `shared/api`; Page models own page-specific state and workflows, while transport-to-domain mapping is introduced only when the shapes or semantics actually differ. Generated types may reduce mechanical duplication, but they do not replace deliberate frontend ownership.

## Deployment and Operations

The Web API 2/OWIN template currently runs as a .NET Framework 4.8 console process self-hosted by Katana on `HttpListener`. Its Host project owns process startup, the OWIN pipeline, dependency injection, Web API configuration, OpenAPI middleware, module composition, database migration ordering, structured logging, and the global exception boundary. An explicit Host-owned descriptor catalog keeps module dependencies, service registration, Controller discovery, and migration order aligned without reflection-based discovery. Customers and Orders each own their HTTP slices, domain state, SQLite Store, and embedded migrations while sharing one database file. A narrowly scoped `BackendVsaOwin.BuildingBlocks.WebApi` project supplies shared Web API 2 transport primitives, including RFC 9457 Problem Details and W3C request tracing, without becoming a home for domain rules. The sibling `BackendVsaOwin.BuildingBlocks.Persistence` project supplies only reusable SQLite connection and DbUp migration infrastructure; module-specific SQL and Stores remain inside their owning modules. Public error responses expose a trace identifier rather than exception details, while Host logs correlate the same identifier with the complete exception. Orders references only the public `Customers.Contracts` assembly and uses `ICustomerLookup` to validate customer identities and capture a customer-name snapshot; it cannot access Customers internals even though the database also enforces the Orders-to-Customers foreign key. Because Web API 2 cannot route the same URI across multiple attribute-routed controller types by HTTP method, action files within each module compose one partial controller while retaining separate handlers and contracts. Exact commands and configuration belong to the [template README](../templates/backend-vsa-webapi2-owin/README.md).

The Vue 3 template provides an independently runnable FSD frontend with complete Orders CRUD, Customer creation and lookup by identifier, Basic and OAuth authentication interfaces, protected routing, and refresh-token rotation. It deliberately uses the minimal `app -> pages -> shared` structure: single-page workflows stay in Page slices, CRUD contracts live in `shared/api`, and application-wide session handling lives in `shared/auth`. Steiger enforces the resulting architecture boundaries. Its Customer surface follows the currently published backend contract and does not invent unsupported list, update, or delete operations. Other templates remain scaffolds. Each future implementation will own its runtime instructions and verification flow.

## Quality Attributes

- **Locality:** a business change should primarily affect one backend slice and the owning frontend Page or smallest confirmed reusable slice.
- **Replaceability:** stack-specific templates remain independent.
- **Traceability:** templates link their implementation choices back to the shared rules.
- **Verifiability:** architecture boundaries are covered by automated checks where the stack supports them.

## Verification Expectations

- Backend templates cover use-case behavior and infrastructure boundaries.
- Frontend templates cover Page models, interactions, Shared contracts, and public APIs.
- Static analysis or architecture tests enforce module and FSD dependency rules where practical.
- A template is described as locally runnable only after its documented build, tests, and startup check pass locally. Release readiness additionally requires those checks to pass in repository automation.

Concrete commands, fixtures, and framework choices stay in the project that owns them. Repository-wide automation may aggregate those commands after runnable implementations exist.

## Decisions and Trade-offs

The repository favors explicit duplication between independent templates over shared runtime abstractions. This costs some maintenance effort but keeps each starter portable. An `adr/` directory should be introduced only when the first durable decision needs rationale beyond this living overview.
