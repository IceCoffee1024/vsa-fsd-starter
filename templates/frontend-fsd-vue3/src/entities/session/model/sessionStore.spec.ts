import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import {
  createBasicAuthorization,
  requestPasswordToken,
  requestRefreshToken,
  verifyBasicAuthorization,
} from '../api/sessionApi'
import { useSessionStore } from './sessionStore'

vi.mock('../api/sessionApi', () => ({
  createBasicAuthorization: vi.fn(() => 'Basic encoded'),
  requestPasswordToken: vi.fn(),
  requestRefreshToken: vi.fn(),
  verifyBasicAuthorization: vi.fn(),
}))

describe('session store', () => {
  beforeEach(() => {
    sessionStorage.clear()
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('validates Basic credentials and keeps them in memory', async () => {
    vi.mocked(verifyBasicAuthorization).mockResolvedValue()
    const store = useSessionStore()

    const signedIn = await store.signInBasic({
      username: 'admin',
      password: 'password',
    })

    expect(createBasicAuthorization).toHaveBeenCalled()
    expect(signedIn).toBe(true)
    expect(store.method).toBe('basic')
    expect(await store.getAuthorizationHeader()).toBe('Basic encoded')
    expect(sessionStorage.length).toBe(0)
  })

  it('stores an OAuth session and rotates its refresh token', async () => {
    vi.mocked(requestPasswordToken).mockResolvedValue({
      access_token: 'access-one',
      refresh_token: 'refresh-one',
      token_type: 'Bearer',
      expires_in: 3600,
    })
    vi.mocked(requestRefreshToken).mockResolvedValue({
      access_token: 'access-two',
      refresh_token: 'refresh-two',
      token_type: 'Bearer',
      expires_in: 3600,
    })
    const store = useSessionStore()

    await store.signInOAuth({
      username: 'admin',
      password: 'password',
      clientId: 'vue-client',
    })
    const refreshed = await store.refreshOAuth()

    expect(refreshed).toBe('Bearer access-two')
    expect(requestRefreshToken).toHaveBeenCalledWith(
      'refresh-one',
      'vue-client',
    )
    expect(sessionStorage.getItem('frontend-fsd-vue3.oauth-session')).toContain(
      'refresh-two',
    )
  })
})
