[English](README.md) | [Simplified Chinese](README.zh-CN.md)

# Frontend FSD: Vue 3

A runnable Vue 3 starter that demonstrates Feature-Sliced Design through complete order management, focused customer workflows, and the authentication flows supported by the Web API 2/OWIN template.

## Stack

- Vue 3 with Composition API and `<script setup lang="ts">`
- Vite and TypeScript
- Vue Router
- Pinia setup stores
- Vitest, Vue Test Utils, and `@pinia/testing`
- Lucide Vue icons (`@lucide/vue`)

## Structure

```text
src/
├── main.ts                      # Vite entrypoint forwarding to the app layer
├── env.d.ts                     # Vite environment type declarations
├── app/
│   ├── index.ts                 # Application bootstrap and cross-cutting wiring
│   ├── App.vue                  # Application shell and primary navigation
│   ├── providers/               # Pinia and future app-wide providers
│   ├── router/                  # Routes and authentication guards
│   └── styles/                  # Global tokens, reset, and shared styles
├── pages/
│   ├── sign-in/                 # Sign-in route composition
│   ├── orders/                  # Order management route composition
│   └── customers/               # Customer registry route composition
├── widgets/
│   └── order-list/              # Order list workflow orchestration
├── features/
│   ├── authenticate/
│   ├── sign-out/
│   ├── create-order/
│   ├── view-order/
│   ├── edit-order/
│   ├── delete-order/
│   ├── batch-create-orders/
│   ├── batch-delete-orders/
│   ├── create-customer/
│   └── find-customer/
├── entities/
│   ├── order/                   # Order model, Store, API, and entity UI
│   ├── customer/                # Customer model, Store, API, and entity UI
│   └── session/                 # Authentication state and token lifecycle
├── shared/
│   ├── api/                     # HTTP and Problem Details adaptation
│   ├── config/                  # Runtime-facing frontend configuration
│   ├── lib/                     # Business-neutral utilities
│   └── ui/                      # Reusable UI primitives
```

| Layer | Responsibility |
| --- | --- |
| `app/` | Global initialization and application-level infrastructure. |
| `pages/` | Route-level composition without implementing use cases. |
| `widgets/` | Larger interface blocks that coordinate multiple features and entities. |
| `features/` | Focused user actions and their interaction state. |
| `entities/` | Business models, shared state, API adapters, and entity-level UI. |
| `shared/` | Reusable capabilities without business-specific meaning. |
| `app/styles/` | Global design tokens, reset rules, and cross-component styles. |

Dependencies point downward through `app -> pages -> widgets -> features -> entities -> shared`. A slice may contain `api`, `model`, and `ui` segments. Its `index.ts` is the public API, while colocated `*.spec.ts` files verify the owning Store, component, or route behavior. Consumers do not reach into another slice's internal segments. The `Order` model is owned by the frontend and is explicitly mapped from transport DTOs.

The Order Pinia store owns the shared collection and applies successful create, update, delete, batch-create, and batch-delete results. The Customer Pinia store keeps create and lookup state independent so either workflow can progress or fail without overwriting the other. Each Feature retains its own form state. The Session Pinia store is the single source of truth for the selected authentication method and credentials.

## Development

Prerequisites:

- Node.js 22.12 or later
- pnpm 11

Install and verify the project:

```powershell
pnpm install
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

`shared/api` converts non-success responses into `ApiError` and preserves RFC 9457 Problem Details fields, validation errors, and `traceId`. Features and widgets render user-safe messages while retaining the trace identifier for support correlation.

## Further Reading

For Vue-specific FSD application guidance, see the official [Vue application architecture article](https://feature-sliced.design/blog/vue-application-architecture). It supplements this template's structure and rules; the README and repository FSD principles remain the source of truth for this project.

## Current Scope

The template covers protected routing, Basic and OAuth sign-in, OAuth refresh-token rotation, local sign-out, complete order management, customer creation and lookup by identifier, client/server validation feedback, selection, confirmation dialogs, and focused Store, route-guard, and component tests. Customer listing and selection, customer update and deletion, generated OpenAPI clients, refresh-token revocation, and automated end-to-end browser tests remain deferred. The omitted customer workflows are not present in the current backend contract and are intentionally not simulated in the frontend.
