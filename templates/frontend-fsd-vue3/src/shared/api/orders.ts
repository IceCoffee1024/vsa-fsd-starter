import { requestJson, requestVoid } from './httpClient'

export interface Order {
  readonly id: string
  readonly customerId: string
  readonly customerName: string
  readonly totalAmount: number
}

export interface CreateOrderInput {
  readonly customerId: string
  readonly totalAmount: number
}

export interface UpdateOrderInput {
  readonly totalAmount: number
}

export interface BatchCreateOrdersInput {
  readonly orders: readonly CreateOrderInput[]
}

export interface BatchCreateOrdersResult {
  readonly createdCount: number
  readonly items: readonly Order[]
}

export interface BatchDeleteOrdersResult {
  readonly requestedCount: number
  readonly deletedCount: number
  readonly missingIds: readonly string[]
}

interface ListOrdersResponse {
  readonly items: readonly Order[]
}

export async function getOrders(): Promise<readonly Order[]> {
  const response = await requestJson<ListOrdersResponse>('orders')
  return response.items
}

export async function postOrder(input: CreateOrderInput): Promise<Order> {
  return requestJson<Order>('orders', {
    method: 'POST',
    body: JSON.stringify(input),
  })
}

export async function getOrder(id: string): Promise<Order> {
  return requestJson<Order>(`orders/${id}`)
}

export async function putOrder(
  id: string,
  input: UpdateOrderInput,
): Promise<Order> {
  return requestJson<Order>(`orders/${id}`, {
    method: 'PUT',
    body: JSON.stringify(input),
  })
}

export function deleteOrder(id: string): Promise<void> {
  return requestVoid(`orders/${id}`, { method: 'DELETE' })
}

export async function postOrdersBatch(
  input: BatchCreateOrdersInput,
): Promise<BatchCreateOrdersResult> {
  return requestJson<BatchCreateOrdersResult>('orders/batch', {
    method: 'POST',
    body: JSON.stringify(input),
  })
}

export async function postOrdersBatchDelete(
  ids: readonly string[],
): Promise<BatchDeleteOrdersResult> {
  return requestJson<BatchDeleteOrdersResult>('orders/batch-delete', {
    method: 'POST',
    body: JSON.stringify({ ids }),
  })
}
