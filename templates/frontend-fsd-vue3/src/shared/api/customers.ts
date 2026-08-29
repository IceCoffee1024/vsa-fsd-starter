import { requestJson } from './httpClient'

export interface Customer {
  readonly id: string
  readonly displayName: string
}

export interface CreateCustomerInput {
  readonly displayName: string
}

export async function getCustomer(id: string): Promise<Customer> {
  return requestJson<Customer>(`customers/${id}`)
}

export async function postCustomer(
  input: CreateCustomerInput,
): Promise<Customer> {
  return requestJson<Customer>('customers', {
    method: 'POST',
    body: JSON.stringify(input),
  })
}
