import { requestJson } from '@/shared/api'
import type { CreateCustomerInput, Customer } from '../model/types'

interface CustomerDto {
  readonly id: string
  readonly displayName: string
}

export async function getCustomer(id: string): Promise<Customer> {
  return toCustomer(await requestJson<CustomerDto>(`customers/${id}`))
}

export async function postCustomer(
  input: CreateCustomerInput,
): Promise<Customer> {
  const response = await requestJson<CustomerDto>('customers', {
    method: 'POST',
    body: JSON.stringify(input),
  })
  return toCustomer(response)
}

function toCustomer(dto: CustomerDto): Customer {
  return {
    id: dto.id,
    displayName: dto.displayName,
  }
}
