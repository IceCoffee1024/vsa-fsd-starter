/// <reference types="vite/client" />

import 'vue-router'

declare module 'vue-router' {
  interface RouteMeta {
    readonly guestOnly?: boolean
    readonly requiresAuth?: boolean
  }
}

interface ImportMetaEnv {
  readonly VITE_BACKEND_BASE_URL?: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
