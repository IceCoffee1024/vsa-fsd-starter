import { createRouter, createWebHistory } from 'vue-router'
import { useSessionStore } from '@/entities/session'
import { pinia } from '../providers/pinia'

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
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
  const session = useSessionStore(pinia)
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
