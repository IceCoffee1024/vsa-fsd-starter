import { computed, shallowRef } from 'vue'
import { defineStore } from 'pinia'
import {
  deleteOrder,
  getOrders,
  normalizeApiError,
  postOrder,
  postOrdersBatch,
  postOrdersBatchDelete,
  putOrder,
  type ApiError,
  type BatchCreateOrdersInput,
  type BatchCreateOrdersResult,
  type BatchDeleteOrdersResult,
  type CreateOrderInput,
  type Order,
  type UpdateOrderInput,
} from '@/shared/api'

type RequestStatus = 'idle' | 'pending' | 'success' | 'error'

export const useOrderStore = defineStore('orders', () => {
  const orders = shallowRef<readonly Order[]>([])
  const listStatus = shallowRef<RequestStatus>('idle')
  const mutationStatus = shallowRef<RequestStatus>('idle')
  const listError = shallowRef<ApiError | null>(null)
  const mutationError = shallowRef<ApiError | null>(null)

  const isLoading = computed(() => listStatus.value === 'pending')
  const isMutating = computed(() => mutationStatus.value === 'pending')

  async function load(): Promise<void> {
    listStatus.value = 'pending'
    listError.value = null

    try {
      orders.value = await getOrders()
      listStatus.value = 'success'
    } catch (error) {
      listError.value = normalizeApiError(error)
      listStatus.value = 'error'
    }
  }

  async function create(input: CreateOrderInput): Promise<Order> {
    return runMutation(async () => {
      const created = await postOrder(input)
      orders.value = [created, ...orders.value]
      return created
    })
  }

  async function update(id: string, input: UpdateOrderInput): Promise<Order> {
    return runMutation(async () => {
      const updated = await putOrder(id, input)
      orders.value = orders.value.map((order) =>
        order.id === id ? updated : order,
      )
      return updated
    })
  }

  async function remove(id: string): Promise<void> {
    await runMutation(async () => {
      await deleteOrder(id)
      orders.value = orders.value.filter((order) => order.id !== id)
    })
  }

  async function createMany(
    input: BatchCreateOrdersInput,
  ): Promise<BatchCreateOrdersResult> {
    return runMutation(async () => {
      const result = await postOrdersBatch(input)
      orders.value = [...result.items, ...orders.value]
      return result
    })
  }

  async function removeMany(
    ids: readonly string[],
  ): Promise<BatchDeleteOrdersResult> {
    return runMutation(async () => {
      const result = await postOrdersBatchDelete(ids)
      const missing = new Set(result.missingIds)
      const deleted = new Set(ids.filter((id) => !missing.has(id)))
      orders.value = orders.value.filter((order) => !deleted.has(order.id))
      return result
    })
  }

  function clearMutationError(): void {
    mutationError.value = null
    if (mutationStatus.value === 'error') {
      mutationStatus.value = 'idle'
    }
  }

  async function runMutation<T>(operation: () => Promise<T>): Promise<T> {
    mutationStatus.value = 'pending'
    mutationError.value = null
    try {
      const result = await operation()
      mutationStatus.value = 'success'
      return result
    } catch (error) {
      mutationError.value = normalizeApiError(error)
      mutationStatus.value = 'error'
      throw mutationError.value
    }
  }

  return {
    orders,
    listStatus,
    mutationStatus,
    listError,
    mutationError,
    isLoading,
    isMutating,
    load,
    create,
    update,
    remove,
    createMany,
    removeMany,
    clearMutationError,
  }
})
