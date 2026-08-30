[English](README.md) | [Simplified Chinese](README.zh-CN.md)

# Frontend FSD: Vue 3

A runnable Vue 3 starter that demonstrates Feature-Sliced Design through complete order management, focused customer workflows, and the authentication flows supported by the Web API 2/OWIN template.

## Stack

- Vue 3 with Composition API and `<script setup lang="ts">`
- Vite and TypeScript
- Vue Router
- Pinia setup stores
- Vitest, Vue Test Utils, and `@pinia/testing`
- Steiger architecture checks
- Lucide Vue icons (`@lucide/vue`)

## Structure

```text
src/
├── main.ts                      # Vite entrypoint forwarding to the app layer
├── env.d.ts                     # Vite environment type declarations
├── app/
│   ├── index.ts                 # Application bootstrap and cross-cutting wiring
│   ├── App.vue                  # Application shell and primary navigation
│   ├── SessionControl.vue       # Application-wide session actions
│   ├── pinia.ts                 # Pinia instance
│   ├── router/                  # Routes and authentication guards
│   └── styles/                  # Global tokens, reset, and shared styles
├── pages/
│   ├── sign-in/
│   │   └── ui/                  # Sign-in page and page-local form
│   ├── orders/
│   │   ├── model/               # Order page state and workflows
│   │   └── ui/                  # Order page, forms, table, and dialogs
│   └── customers/
│       ├── model/               # Customer page state and workflows
│       └── ui/                  # Customer page, forms, and summary
├── shared/
│   ├── api/                     # HTTP client, CRUD contracts, and Problem Details
│   ├── auth/                    # Session state and authentication requests
│   ├── config/                  # Runtime-facing frontend configuration
│   ├── lib/                     # Business-neutral utilities
│   └── ui/                      # Reusable UI primitives
```

| Layer | Responsibility |
| --- | --- |
| `app/` | Global initialization and application-level infrastructure. |
| `pages/` | Route-level UI, state, validation, and workflows owned by one screen. |
| `shared/` | Reusable infrastructure, backend CRUD contracts, authentication, and business-neutral UI or utilities; it does not own product workflows. |
| `app/styles/` | Global design tokens, reset rules, and cross-component styles. |

The current template intentionally uses the minimal FSD direction `app -> pages -> shared`. FSD layers are optional: add `features` only for a stable interaction already reused by multiple pages, add `entities` only for a stable domain model with multiple consumers, and avoid `widgets` unless an exceptional reusable composition has a clear boundary. Single-page behavior remains in its page slice even when it contains substantial UI or business flow.

Each page slice exposes its route component through `index.ts`; code inside that slice uses relative imports. Shared defines a public API per segment, such as `shared/api` and `shared/auth`, and consumers do not reach into segment internals. Domain-based filenames describe their concern instead of using generic names such as `types.ts` or `utils.ts`.

The Order page Store owns its collection and applies successful create, update, delete, batch-create, and batch-delete results. The Customer page Store keeps create and lookup state independent so either workflow can progress or fail without overwriting the other. Forms keep local interaction state. CRUD request functions and wire types live in `shared/api`; the application-wide Session Store and token lifecycle live in `shared/auth`.

## Development

Prerequisites:

- Node.js 22.12 or later
- pnpm 11

Install and verify the project:

```powershell
pnpm install
pnpm check:architecture
pnpm typecheck
pnpm test
pnpm build
```

Start the frontend:

```powershell
Copy-Item .env.example .env.local
pnpm dev
```

The development server listens on `http://localhost:5173`.

## Backend Proxy

The browser calls `/backend/api` and `/backend/oauth/token`. During development, Vite proxies `/backend` to `BACKEND_API_URL` and removes the prefix. Authentication headers are supplied by the frontend session rather than injected by the proxy, so both Basic and OAuth behavior remain visible and testable.

The defaults in `.env.example` match the Web API 2/OWIN starter, but this frontend remains an independent template. A production deployment must provide its own same-origin gateway, BFF, or API authentication strategy; Vite's development proxy is not part of the production build.

The router uses HTML5 history through `createWebHistory`. In production, the static host or gateway must serve `index.html` for non-file frontend routes such as `/sign-in` and `/orders`. Asset and backend paths, including `/backend`, must bypass this SPA fallback.

`VITE_BACKEND_BASE_URL` is intentionally public and controls the browser-visible backend prefix. Do not place secrets in any `VITE_*` variable.

## Authentication

The sign-in page supports the two schemes implemented by the backend template:

- Basic authentication validates the credentials against a protected endpoint. The resulting Authorization header remains in memory and is discarded by a page reload.
- OAuth 2.0 uses the backend's demo password grant and optional public `client_id`. The access token, rotating refresh token, expiry, username, and client binding are stored in `sessionStorage`; an expiring access token is refreshed before an API request, and the header also provides a manual refresh action.

Signing out clears the local session. The backend does not expose a refresh-token revocation endpoint, so this starter cannot perform server-side logout. The password grant and insecure HTTP are compatibility choices of the accompanying legacy backend template, not recommendations for a new public browser client.

## Error Contract

`shared/api` converts non-success responses into `ApiError` and preserves RFC 9457 Problem Details fields, validation errors, and `traceId`. Page UI renders user-safe messages while retaining the trace identifier for support correlation.

## Further Reading

This template follows FSD v2.1; use the current [official FSD documentation](https://fsd.how) as the external reference. The older official [Vue application architecture article](https://feature-sliced.design/blog/vue-application-architecture) remains useful supplementary reading, but some examples use an earlier, more layer-heavy style. This README and the repository FSD principles remain authoritative for the template's concrete choices.

## Current Scope

The template covers protected routing, Basic and OAuth sign-in, OAuth refresh-token rotation, local sign-out, complete order management, customer creation and lookup by identifier, client/server validation feedback, selection, confirmation dialogs, and focused Store, route-guard, and component tests. Customer listing and selection, customer update and deletion, generated OpenAPI clients, refresh-token revocation, and automated end-to-end browser tests remain deferred. The omitted customer workflows are not present in the current backend contract and are intentionally not simulated in the frontend.
