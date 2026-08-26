---
state: Current
last_updated: "2026-08-26"
---

[English](architecture-overview.md) | [简体中文](architecture-overview.zh-CN.md)

# Architecture Overview

## Context and Drivers

This repository teaches and demonstrates how Vertical Slice Architecture (VSA) and Feature-Sliced Design (FSD) can coexist in full-stack systems. It separates reusable guidance, minimal stack-specific starters, and production-oriented examples so that educational business code does not leak into clean templates.

## Repository Boundaries

- `docs/` is the canonical source for rules shared by all stacks.
- Each completed directory under `templates/` is an independent starter and must document its own runtime and commands.
- Each completed directory under `examples/` is an independently runnable full-stack application assembled from the shared principles.
- Templates and examples may reference the documentation, but they must not depend on runtime code from another template or example.
- An example may copy and adapt template code, but templates must not depend on examples.
- Stack-specific commands, configuration, and troubleshooting stay with their owning project.

## Components and Responsibilities

| Component | Responsibility |
| --- | --- |
| `docs/` | Own cross-stack architecture principles, dependency rules, and verification strategy. |
| `templates/backend-*` | Demonstrate VSA and modular-monolith boundaries in one backend stack. The Web API 2/OWIN template is currently implemented. |
| `templates/frontend-*` | Demonstrate FSD dependency direction and public APIs in one frontend stack. |
| `examples/legacy-ordering/` | Demonstrate end-to-end modernization concerns without turning templates into a sample product. |
| `scripts/` | Own repository-wide automation after runnable targets exist. |

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
FSD page or widget
  -> one or more FSD features
    -> HTTP API contract
      -> one VSA endpoint and use-case slice
```

- A simple command may map one frontend feature to one backend slice.
- A page may compose several frontend features and call several backend slices.
- A backend slice may serve multiple frontend entry points when its contract and authorization rules are the same.
- Read models should be shaped for the use case rather than exposing persistence entities.

## Interfaces and Data

Frontend and backend projects communicate through explicit HTTP API contracts. The backend owns its published schema and compatibility policy. The frontend consumes that contract without importing backend implementation code and owns adaptation from transport DTOs into its entity or feature models. Generated types may reduce mechanical duplication, but they do not replace either side's domain model.

## Deployment and Operations

The Web API 2/OWIN template currently runs as a .NET Framework 4.8 console process self-hosted by Katana on `HttpListener`. Its Host project owns process startup, the OWIN pipeline, dependency injection, Web API configuration, OpenAPI middleware, module composition, database migration ordering, structured logging, and the global exception boundary. An explicit Host-owned descriptor catalog keeps module dependencies, service registration, Controller discovery, and migration order aligned without reflection-based discovery. Customers and Orders each own their HTTP slices, domain state, SQLite Store, and embedded migrations while sharing one database file. A narrowly scoped `BackendVsaOwin.BuildingBlocks.WebApi` project supplies shared Web API 2 transport primitives, including RFC 9457 Problem Details and W3C request tracing, without becoming a home for domain rules. The sibling `BackendVsaOwin.BuildingBlocks.Persistence` project supplies only reusable SQLite connection and DbUp migration infrastructure; module-specific SQL and Stores remain inside their owning modules. Public error responses expose a trace identifier rather than exception details, while Host logs correlate the same identifier with the complete exception. Orders references only the public `Customers.Contracts` assembly and uses `ICustomerLookup` to validate customer identities and capture a customer-name snapshot; it cannot access Customers internals even though the database also enforces the Orders-to-Customers foreign key. Because Web API 2 cannot route the same URI across multiple attribute-routed controller types by HTTP method, action files within each module compose one partial controller while retaining separate handlers and contracts. Exact commands and configuration belong to the [template README](../templates/backend-vsa-webapi2-owin/README.md).

Other templates and the full-stack example remain scaffolds. Each future implementation will own its runtime instructions; full-stack examples will also document their composed startup and deployment flow.

## Quality Attributes

- **Locality:** a business change should primarily affect one backend slice and the smallest relevant frontend slices.
- **Replaceability:** stack-specific templates remain independent.
- **Traceability:** examples link their implementation choices back to the shared rules.
- **Verifiability:** architecture boundaries are covered by automated checks where the stack supports them.

## Verification Expectations

- Backend templates cover use-case behavior and infrastructure boundaries.
- Frontend templates cover feature models, interactions, and public APIs.
- Full-stack examples cover API contracts and critical end-to-end flows.
- Static analysis or architecture tests enforce module and FSD dependency rules where practical.
- A template is described as locally runnable only after its documented build, tests, and startup check pass locally. Release readiness additionally requires those checks to pass in repository automation.

Concrete commands, fixtures, and framework choices stay in the project that owns them. Repository-wide automation may aggregate those commands after runnable implementations exist.

## Decisions and Trade-offs

The repository favors explicit duplication between independent templates over shared runtime abstractions. This costs some maintenance effort but keeps each starter portable. An `adr/` directory should be introduced only when the first durable decision needs rationale beyond this living overview.
