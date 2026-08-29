import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import {
  createBasicAuthorization,
  requestPasswordToken,
  requestRefreshToken,
  verifyBasicAuthorization,
} from './authentication'
import { useSessionStore } from './session'

vi.mock('./authentication', () => ({
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

  it('refreshes an expired OAuth session restored from browser storage', async () => {
    storeExpiredOAuthSession()
    vi.mocked(requestRefreshToken).mockResolvedValue({
      access_token: 'access-two',
      refresh_token: 'refresh-two',
      token_type: 'Bearer',
      expires_in: 3600,
    })
    const store = useSessionStore()

    const authenticated = await store.ensureAuthenticated()

    expect(authenticated).toBe(true)
    expect(requestRefreshToken).toHaveBeenCalledWith(
      'refresh-one',
      'vue-client',
    )
    expect(store.oauthToken?.accessToken).toBe('access-two')
  })

  it('clears an expired restored session when token refresh fails', async () => {
    storeExpiredOAuthSession()
    vi.mocked(requestRefreshToken).mockRejectedValue(
      new Error('Refresh token rejected'),
    )
    const store = useSessionStore()

    const authenticated = await store.ensureAuthenticated()

    expect(authenticated).toBe(false)
    expect(store.isAuthenticated).toBe(false)
    expect(sessionStorage.getItem('frontend-fsd-vue3.oauth-session')).toBeNull()
  })
})

function storeExpiredOAuthSession(): void {
  sessionStorage.setItem(
    'frontend-fsd-vue3.oauth-session',
    JSON.stringify({
      accessToken: 'access-one',
      refreshToken: 'refresh-one',
      tokenType: 'Bearer',
      expiresAt: Date.now() - 1,
      clientId: 'vue-client',
      username: 'admin',
    }),
  )
}
