import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { ApiError } from '@/shared/api'
import { getCustomer, postCustomer } from '../api/customerApi'
import { useCustomerStore } from './customerStore'

vi.mock('../api/customerApi', () => ({
  getCustomer: vi.fn(),
  postCustomer: vi.fn(),
}))

const customer = {
  id: '20000000-0000-4000-8000-000000000001',
  displayName: 'Ada Lovelace',
}

describe('customer store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('stores a newly created customer', async () => {
    vi.mocked(postCustomer).mockResolvedValue(customer)
    const store = useCustomerStore()

    const created = await store.create({ displayName: 'Ada Lovelace' })

    expect(postCustomer).toHaveBeenCalledWith({ displayName: 'Ada Lovelace' })
    expect(created).toEqual(customer)
    expect(store.createdCustomer).toEqual(customer)
    expect(store.createStatus).toBe('success')
  })

  it('stores a customer returned by identifier lookup', async () => {
    vi.mocked(getCustomer).mockResolvedValue(customer)
    const store = useCustomerStore()

    const found = await store.find(customer.id)

    expect(getCustomer).toHaveBeenCalledWith(customer.id)
    expect(found).toEqual(customer)
    expect(store.lookupCustomer).toEqual(customer)
    expect(store.lookupStatus).toBe('success')
  })

  it('normalizes a failed lookup and clears stale results', async () => {
    vi.mocked(getCustomer)
      .mockResolvedValueOnce(customer)
      .mockRejectedValueOnce(new ApiError(404, null, 'Customer not found.'))
    const store = useCustomerStore()
    await store.find(customer.id)

    await expect(store.find(customer.id)).rejects.toThrow('Customer not found.')

    expect(store.lookupCustomer).toBeNull()
    expect(store.lookupError?.status).toBe(404)
    expect(store.lookupStatus).toBe('error')
  })
})
