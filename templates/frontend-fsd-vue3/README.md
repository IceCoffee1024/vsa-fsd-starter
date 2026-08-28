[English](README.md) | [Simplified Chinese](README.zh-CN.md)

# Frontend FSD: Vue 3

A runnable Vue 3 starter that demonstrates Feature-Sliced Design through complete order management and the authentication flows supported by the Web API 2/OWIN template.

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
├── app/                         # Pinia provider, router, and authentication guards
├── pages/                       # Sign-in and order route composition
├── widgets/order-list/          # CRUD toolbar, selection, and dialog orchestration
├── features/                    # Authentication and focused order operations
├── entities/
│   ├── order/                   # Order model, API adapter, Store, and table UI
│   └── session/                 # Basic/OAuth API, session state, and token rotation
├── shared/
│   ├── api/                     # HTTP and Problem Details adaptation
│   ├── config/                  # Runtime-facing frontend configuration
│   ├── lib/                     # Business-neutral utilities
│   └── ui/                      # Reusable dialog primitive
└── styles/                      # Global tokens and reset
```

Dependencies point downward through `app -> pages -> widgets -> features -> entities -> shared`. Consumers import a slice through its `index.ts`; they do not reach into another slice's internal segments. The `Order` model is owned by the frontend and is explicitly mapped from transport DTOs.

The Order Pinia store owns the shared collection and applies successful create, update, delete, batch-create, and batch-delete results. Each Feature retains its own form and dialog state. The Session Pinia store is the single source of truth for the selected authentication method and credentials.

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

## Current Scope

The template covers protected routing, Basic and OAuth sign-in, OAuth refresh-token rotation, local sign-out, order listing and detail retrieval, create, update, delete, batch create, batch delete, client/server validation feedback, selection, confirmation dialogs, and focused Store, route-guard, and component tests. Customer lookup, generated OpenAPI clients, refresh-token revocation, and automated end-to-end browser tests remain deferred.
