export type AuthenticationMethod = 'basic' | 'oauth'

export interface BasicSignInInput {
  readonly username: string
  readonly password: string
}

export interface OAuthSignInInput extends BasicSignInInput {
  readonly clientId: string
}

export interface OAuthToken {
  readonly accessToken: string
  readonly refreshToken: string
  readonly tokenType: string
  readonly expiresAt: number
  readonly clientId: string
  readonly username: string
}

export interface OAuthTokenResponse {
  readonly access_token: string
  readonly refresh_token: string
  readonly token_type: string
  readonly expires_in: number
}

export type AuthenticationStatus = 'idle' | 'pending' | 'success' | 'error'
