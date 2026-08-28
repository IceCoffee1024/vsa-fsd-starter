import { createMemoryHistory } from 'vue-router'
import { describe, expect, it, vi } from 'vitest'
import { createAppRouter } from './index'

describe('authentication route guard', () => {
  it('redirects direct protected navigation to sign-in', async () => {
    const session = createSession(false, false)
    const router = createAppRouter({
      history: createMemoryHistory(),
      getSession: () => session,
    })

    await router.push('/orders')

    expect(router.currentRoute.value.name).toBe('sign-in')
    expect(router.currentRoute.value.query.redirect).toBe('/orders')
    expect(session.ensureAuthenticated).toHaveBeenCalledOnce()
  })

  it('allows authenticated navigation to a protected route', async () => {
    const session = createSession(true, true)
    const router = createAppRouter({
      history: createMemoryHistory(),
      getSession: () => session,
    })

    await router.push('/orders')

    expect(router.currentRoute.value.name).toBe('orders')
    expect(session.ensureAuthenticated).toHaveBeenCalledOnce()
  })

  it('redirects an authenticated user away from the guest-only page', async () => {
    const session = createSession(true, true)
    const router = createAppRouter({
      history: createMemoryHistory(),
      getSession: () => session,
    })

    await router.push('/sign-in')

    expect(router.currentRoute.value.name).toBe('orders')
    expect(session.ensureAuthenticated).toHaveBeenCalledOnce()
  })
})

function createSession(isAuthenticated: boolean, isValid: boolean) {
  return {
    isAuthenticated,
    ensureAuthenticated: vi.fn().mockResolvedValue(isValid),
  }
}
