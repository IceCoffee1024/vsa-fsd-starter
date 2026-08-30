---
state: Current
last_updated: "2026-08-30"
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

Concrete layouts, runtime instructions, and stack-specific decisions are maintained by the implemented templates: [Web API 2/OWIN](../templates/backend-vsa-webapi2-owin/README.md) and [Vue 3](../templates/frontend-fsd-vue3/README.md). Those READMEs are the authoritative sources for their implementations.

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

## Implementation Status

The Web API 2/OWIN backend and Vue 3 frontend are independently runnable reference templates. The backend demonstrates use-case slices, explicit module contracts, host-owned composition, shared technical building blocks, persistence, authentication, and observable error handling. The frontend demonstrates the minimal `app -> pages -> shared` FSD structure, public APIs, page-owned workflows, application-wide session handling, and architecture enforcement. Their exact capabilities, layouts, commands, and operational constraints belong to their respective template READMEs. Other template directories remain scaffolds and do not yet claim runnable behavior.

## Quality Attributes

- **Locality:** a business change should primarily affect one backend slice and the owning frontend Page or smallest confirmed reusable slice.
- **Replaceability:** stack-specific templates remain independent.
- **Traceability:** templates link their implementation choices back to the shared rules.
- **Verifiability:** architecture boundaries are covered by automated checks where the stack supports them.

## Verification Expectations

- Backend templates cover use-case behavior and infrastructure boundaries.
- Frontend templates cover Page models, interactions, Shared contracts, and public APIs.
- Static analysis or architecture tests enforce module and FSD dependency rules where practical.
- A template is described as locally runnable only after its documented build, tests, and startup check pass locally.

Concrete commands, fixtures, and framework choices stay in the project that owns them. A repository adopting a template owns its own release and downstream automation policy.

## Decisions and Trade-offs

The repository favors explicit duplication between independent templates over shared runtime abstractions. This costs some maintenance effort but keeps each starter portable. An `adr/` directory should be introduced only when the first durable decision needs rationale beyond this living overview.
