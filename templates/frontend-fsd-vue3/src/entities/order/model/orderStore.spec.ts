import { beforeEach, describe, expect, it, vi } from 'vitest'
import { createPinia, setActivePinia } from 'pinia'
import { ApiError } from '@/shared/api'
import {
  deleteOrder,
  getOrders,
  postOrder,
  postOrdersBatch,
  postOrdersBatchDelete,
  putOrder,
} from '../api/orderApi'
import { useOrderStore } from './orderStore'

vi.mock('../api/orderApi', () => ({
  deleteOrder: vi.fn(),
  getOrders: vi.fn(),
  postOrder: vi.fn(),
  postOrdersBatch: vi.fn(),
  postOrdersBatchDelete: vi.fn(),
  putOrder: vi.fn(),
}))

const existingOrder = {
  id: '10000000-0000-4000-8000-000000000001',
  customerId: '20000000-0000-4000-8000-000000000001',
  customerName: 'Ada Lovelace',
  totalAmount: 42.5,
}

const secondOrder = {
  ...existingOrder,
  id: '10000000-0000-4000-8000-000000000002',
  totalAmount: 95,
}

describe('order store', () => {
  beforeEach(() => {
    setActivePinia(createPinia())
    vi.clearAllMocks()
  })

  it('loads, creates, updates, and deletes orders', async () => {
    vi.mocked(getOrders).mockResolvedValue([existingOrder])
    vi.mocked(postOrder).mockResolvedValue(secondOrder)
    vi.mocked(putOrder).mockResolvedValue({ ...secondOrder, totalAmount: 125 })
    vi.mocked(deleteOrder).mockResolvedValue()
    const store = useOrderStore()

    await store.load()
    await store.create({
      customerId: secondOrder.customerId,
      totalAmount: secondOrder.totalAmount,
    })
    await store.update(secondOrder.id, { totalAmount: 125 })
    await store.remove(existingOrder.id)

    expect(store.listStatus).toBe('success')
    expect(store.mutationStatus).toBe('success')
    expect(store.orders).toEqual([{ ...secondOrder, totalAmount: 125 }])
  })

  it('adds and removes batches while preserving missing identifiers', async () => {
    vi.mocked(postOrdersBatch).mockResolvedValue({
      createdCount: 2,
      items: [existingOrder, secondOrder],
    })
    vi.mocked(postOrdersBatchDelete).mockResolvedValue({
      requestedCount: 2,
      deletedCount: 1,
      missingIds: [secondOrder.id],
    })
    const store = useOrderStore()

    await store.createMany({
      orders: [
        {
          customerId: existingOrder.customerId,
          totalAmount: existingOrder.totalAmount,
        },
        {
          customerId: secondOrder.customerId,
          totalAmount: secondOrder.totalAmount,
        },
      ],
    })
    const result = await store.removeMany([existingOrder.id, secondOrder.id])

    expect(result.missingIds).toEqual([secondOrder.id])
    expect(store.orders).toEqual([secondOrder])
  })

  it('retains problem details when a mutation fails', async () => {
    const problem = {
      title: 'Validation failed',
      status: 400,
      traceId: 'trace-123',
      errors: {
        totalAmount: ['The amount must be greater than zero.'],
      },
    }
    vi.mocked(postOrder).mockRejectedValue(
      new ApiError(400, problem, 'Request failed'),
    )
    const store = useOrderStore()

    await expect(
      store.create({
        customerId: existingOrder.customerId,
        totalAmount: 0,
      }),
    ).rejects.toBeInstanceOf(ApiError)

    expect(store.mutationStatus).toBe('error')
    expect(store.mutationError?.problem).toEqual(problem)
  })
})
