import {
  createRouter,
  createWebHistory,
  type RouterHistory,
} from 'vue-router'
import { useSessionStore } from '@/entities/session'
import { pinia } from '../providers/pinia'

interface AuthenticationSession {
  readonly isAuthenticated: boolean
  ensureAuthenticated(): Promise<boolean>
}

interface CreateAppRouterOptions {
  history?: RouterHistory
  getSession?: () => AuthenticationSession
}

export function createAppRouter(
  options: CreateAppRouterOptions = {},
): ReturnType<typeof createRouter> {
  const router = createRouter({
    history: options.history ?? createWebHistory(import.meta.env.BASE_URL),
    routes: [
      {
        path: '/',
        redirect: '/orders',
      },
      {
        path: '/sign-in',
        name: 'sign-in',
        component: () => import('@/pages/sign-in'),
        meta: { guestOnly: true },
      },
      {
        path: '/orders',
        name: 'orders',
        component: () => import('@/pages/orders'),
        meta: { requiresAuth: true },
      },
      {
        path: '/:pathMatch(.*)*',
        redirect: '/orders',
      },
    ],
  })

  router.beforeEach(async (to) => {
    const session = options.getSession?.() ?? useSessionStore(pinia)
    if (to.meta.requiresAuth && !(await session.ensureAuthenticated())) {
      return {
        name: 'sign-in',
        query: { redirect: to.fullPath },
      }
    }
    if (to.meta.guestOnly && session.isAuthenticated) {
      return { name: 'orders' }
    }
  })

  return router
}

export const router = createAppRouter()
