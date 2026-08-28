export interface Customer {
  readonly id: string
  readonly displayName: string
}

export interface CreateCustomerInput {
  readonly displayName: string
}

export type CustomerRequestStatus = 'idle' | 'pending' | 'success' | 'error'
