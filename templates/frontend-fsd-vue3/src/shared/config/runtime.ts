const configuredBackendBaseUrl = import.meta.env.VITE_BACKEND_BASE_URL?.trim()

export const backendBaseUrl = (
  configuredBackendBaseUrl || '/backend'
).replace(/\/$/, '')

export const apiBaseUrl = `${backendBaseUrl}/api`
