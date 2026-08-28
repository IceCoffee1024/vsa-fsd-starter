import { computed, shallowRef } from 'vue'
import { defineStore } from 'pinia'
import { normalizeApiError, type ApiError } from '@/shared/api'
import {
  createBasicAuthorization,
  requestPasswordToken,
  requestRefreshToken,
  verifyBasicAuthorization,
} from '../api/sessionApi'
import type {
  AuthenticationMethod,
  AuthenticationStatus,
  BasicSignInInput,
  OAuthSignInInput,
  OAuthToken,
  OAuthTokenResponse,
} from './types'

const oauthStorageKey = 'frontend-fsd-vue3.oauth-session'
const refreshWindowMilliseconds = 30_000

export const useSessionStore = defineStore('session', () => {
  const method = shallowRef<AuthenticationMethod | null>(null)
  const username = shallowRef('')
  const basicAuthorization = shallowRef<string | null>(null)
  const oauthToken = shallowRef<OAuthToken | null>(restoreOAuthToken())
  const signInStatus = shallowRef<AuthenticationStatus>('idle')
  const refreshStatus = shallowRef<AuthenticationStatus>('idle')
  const authenticationError = shallowRef<ApiError | null>(null)
  let refreshPromise: Promise<string | null> | null = null

  if (oauthToken.value) {
    method.value = 'oauth'
    username.value = oauthToken.value.username
  }

  const isAuthenticated = computed(() => method.value !== null)
  const isSigningIn = computed(() => signInStatus.value === 'pending')
  const isRefreshing = computed(() => refreshStatus.value === 'pending')

  async function signInBasic(input: BasicSignInInput): Promise<boolean> {
    signInStatus.value = 'pending'
    authenticationError.value = null
    const authorization = createBasicAuthorization(input)

    try {
      await verifyBasicAuthorization(authorization)
      clearOAuthToken()
      basicAuthorization.value = authorization
      method.value = 'basic'
      username.value = input.username
      signInStatus.value = 'success'
      return true
    } catch (error) {
      authenticationError.value = normalizeApiError(error)
      signInStatus.value = 'error'
      return false
    }
  }

  async function signInOAuth(input: OAuthSignInInput): Promise<boolean> {
    signInStatus.value = 'pending'
    authenticationError.value = null

    try {
      const response = await requestPasswordToken(input)
      setOAuthToken(response, input.username, input.clientId)
      basicAuthorization.value = null
      method.value = 'oauth'
      username.value = input.username
      signInStatus.value = 'success'
      return true
    } catch (error) {
      authenticationError.value = normalizeApiError(error)
      signInStatus.value = 'error'
      return false
    }
  }

  async function refreshOAuth(): Promise<string | null> {
    if (!oauthToken.value) {
      return null
    }
    if (refreshPromise) {
      return refreshPromise
    }

    refreshPromise = refreshOAuthCore()
    try {
      return await refreshPromise
    } finally {
      refreshPromise = null
    }
  }

  async function refreshOAuthCore(): Promise<string | null> {
    const current = oauthToken.value
    if (!current) {
      return null
    }

    refreshStatus.value = 'pending'
    authenticationError.value = null
    try {
      const response = await requestRefreshToken(
        current.refreshToken,
        current.clientId,
      )
      setOAuthToken(response, current.username, current.clientId)
      refreshStatus.value = 'success'
      return `Bearer ${response.access_token}`
    } catch (error) {
      authenticationError.value = normalizeApiError(error)
      refreshStatus.value = 'error'
      signOut()
      throw error
    }
  }

  async function getAuthorizationHeader(): Promise<string | null> {
    if (method.value === 'basic') {
      return basicAuthorization.value
    }

    const token = oauthToken.value
    if (!token) {
      return null
    }
    if (token.expiresAt - Date.now() <= refreshWindowMilliseconds) {
      return refreshOAuth()
    }
    return `${token.tokenType} ${token.accessToken}`
  }

  async function ensureAuthenticated(): Promise<boolean> {
    if (!isAuthenticated.value) {
      return false
    }
    try {
      return (await getAuthorizationHeader()) !== null
    } catch {
      return false
    }
  }

  function signOut(): void {
    method.value = null
    username.value = ''
    basicAuthorization.value = null
    clearOAuthToken()
    signInStatus.value = 'idle'
    refreshStatus.value = 'idle'
  }

  function clearError(): void {
    authenticationError.value = null
    if (signInStatus.value === 'error') {
      signInStatus.value = 'idle'
    }
  }

  function setOAuthToken(
    response: OAuthTokenResponse,
    tokenUsername: string,
    clientId: string,
  ): void {
    oauthToken.value = {
      accessToken: response.access_token,
      refreshToken: response.refresh_token,
      tokenType: response.token_type || 'Bearer',
      expiresAt: Date.now() + response.expires_in * 1000,
      clientId,
      username: tokenUsername,
    }
    sessionStorage.setItem(oauthStorageKey, JSON.stringify(oauthToken.value))
  }

  function clearOAuthToken(): void {
    oauthToken.value = null
    sessionStorage.removeItem(oauthStorageKey)
  }

  return {
    method,
    username,
    oauthToken,
    signInStatus,
    refreshStatus,
    authenticationError,
    isAuthenticated,
    isSigningIn,
    isRefreshing,
    signInBasic,
    signInOAuth,
    refreshOAuth,
    getAuthorizationHeader,
    ensureAuthenticated,
    signOut,
    clearError,
  }
})

function restoreOAuthToken(): OAuthToken | null {
  const stored = sessionStorage.getItem(oauthStorageKey)
  if (!stored) {
    return null
  }

  try {
    const candidate = JSON.parse(stored) as Partial<OAuthToken>
    if (
      typeof candidate.accessToken === 'string' &&
      typeof candidate.refreshToken === 'string' &&
      typeof candidate.tokenType === 'string' &&
      typeof candidate.expiresAt === 'number' &&
      typeof candidate.clientId === 'string' &&
      typeof candidate.username === 'string'
    ) {
      return candidate as OAuthToken
    }
  } catch {
    // Invalid browser state is discarded below.
  }

  sessionStorage.removeItem(oauthStorageKey)
  return null
}
