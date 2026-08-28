import { apiBaseUrl } from '@/shared/config'

type AuthorizationHeaderProvider = () =>
  | string
  | null
  | Promise<string | null>

interface RequestOptions {
  readonly authorization?: string | null
  readonly notifyUnauthorized?: boolean
}

let authorizationHeaderProvider: AuthorizationHeaderProvider = () => null
let unauthorizedHandler: () => void | Promise<void> = () => undefined

export interface ProblemDetails {
  readonly type?: string
  readonly title?: string
  readonly status?: number
  readonly detail?: string
  readonly instance?: string
  readonly traceId?: string
  readonly errors?: Readonly<Record<string, readonly string[]>>
}

export class ApiError extends Error {
  public readonly status: number
  public readonly problem: ProblemDetails | null

  public constructor(
    status: number,
    problem: ProblemDetails | null,
    fallbackMessage: string,
  ) {
    super(problem?.detail || problem?.title || fallbackMessage)
    this.name = 'ApiError'
    this.status = status
    this.problem = problem
  }
}

export function configureHttpAuthentication(options: {
  readonly getAuthorizationHeader: AuthorizationHeaderProvider
  readonly onUnauthorized: () => void | Promise<void>
}): void {
  authorizationHeaderProvider = options.getAuthorizationHeader
  unauthorizedHandler = options.onUnauthorized
}

export async function requestJson<T>(
  path: string,
  init: RequestInit = {},
  options: RequestOptions = {},
): Promise<T> {
  const response = await request(path, init, options)
  return (await response.json()) as T
}

export async function requestVoid(
  path: string,
  init: RequestInit = {},
  options: RequestOptions = {},
): Promise<void> {
  await request(path, init, options)
}

async function request(
  path: string,
  init: RequestInit,
  options: RequestOptions,
): Promise<Response> {
  const headers = new Headers(init.headers)
  headers.set('Accept', 'application/json')
  if (init.body !== undefined && !headers.has('Content-Type')) {
    headers.set('Content-Type', 'application/json')
  }

  const authorization =
    options.authorization === undefined
      ? await authorizationHeaderProvider()
      : options.authorization
  if (authorization) {
    headers.set('Authorization', authorization)
  }

  const response = await fetch(`${apiBaseUrl}/${path.replace(/^\//, '')}`, {
    ...init,
    headers,
  })

  if (!response.ok) {
    if (response.status === 401 && options.notifyUnauthorized !== false) {
      await unauthorizedHandler()
    }
    throw await createApiError(response)
  }

  return response
}

export function normalizeApiError(error: unknown): ApiError {
  if (error instanceof ApiError) {
    return error
  }

  return new ApiError(
    0,
    null,
    error instanceof Error
      ? error.message
      : 'The request could not be completed.',
  )
}

async function createApiError(response: Response): Promise<ApiError> {
  const fallbackMessage = `The API returned HTTP ${response.status}.`
  const contentType = response.headers.get('content-type') || ''
  if (!contentType.includes('json')) {
    return new ApiError(response.status, null, fallbackMessage)
  }

  try {
    const problem = (await response.json()) as ProblemDetails
    return new ApiError(response.status, problem, fallbackMessage)
  } catch {
    return new ApiError(response.status, null, fallbackMessage)
  }
}
