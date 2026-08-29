import { ApiError, requestJson } from '@/shared/api'
import { backendBaseUrl } from '@/shared/config'

export interface BasicSignInInput {
  readonly username: string
  readonly password: string
}

export interface OAuthSignInInput extends BasicSignInInput {
  readonly clientId: string
}

export interface OAuthTokenResponse {
  readonly access_token: string
  readonly refresh_token: string
  readonly token_type: string
  readonly expires_in: number
}

interface OAuthErrorResponse {
  readonly error?: string
  readonly error_description?: string
}

export function createBasicAuthorization(input: BasicSignInInput): string {
  const bytes = new TextEncoder().encode(`${input.username}:${input.password}`)
  let binary = ''
  for (const byte of bytes) {
    binary += String.fromCharCode(byte)
  }
  return `Basic ${btoa(binary)}`
}

export async function verifyBasicAuthorization(
  authorization: string,
): Promise<void> {
  await requestJson<unknown>(
    'orders',
    { method: 'GET' },
    { authorization, notifyUnauthorized: false },
  )
}

export function requestPasswordToken(
  input: OAuthSignInInput,
): Promise<OAuthTokenResponse> {
  return requestToken({
    grant_type: 'password',
    username: input.username,
    password: input.password,
    client_id: input.clientId,
  })
}

export function requestRefreshToken(
  refreshToken: string,
  clientId: string,
): Promise<OAuthTokenResponse> {
  return requestToken({
    grant_type: 'refresh_token',
    refresh_token: refreshToken,
    client_id: clientId,
  })
}

async function requestToken(
  fields: Readonly<Record<string, string>>,
): Promise<OAuthTokenResponse> {
  const body = new URLSearchParams()
  for (const [name, value] of Object.entries(fields)) {
    if (value) {
      body.set(name, value)
    }
  }

  const response = await fetch(`${backendBaseUrl}/oauth/token`, {
    method: 'POST',
    headers: {
      Accept: 'application/json',
      'Content-Type': 'application/x-www-form-urlencoded',
    },
    body,
  })

  if (response.ok) {
    return (await response.json()) as OAuthTokenResponse
  }

  let oauthError: OAuthErrorResponse = {}
  try {
    oauthError = (await response.json()) as OAuthErrorResponse
  } catch {
    // The fallback below remains useful when an intermediary returns non-JSON.
  }

  throw new ApiError(
    response.status,
    null,
    oauthError.error_description ||
      oauthError.error ||
      `The token endpoint returned HTTP ${response.status}.`,
  )
}
