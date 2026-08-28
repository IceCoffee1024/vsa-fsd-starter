import { requestJson, requestVoid } from '@/shared/api'
import type {
  BatchCreateOrdersInput,
  BatchCreateOrdersResult,
  BatchDeleteOrdersResult,
  CreateOrderInput,
  Order,
  UpdateOrderInput,
} from '../model/types'

interface OrderDto {
  readonly id: string
  readonly customerId: string
  readonly customerName: string
  readonly totalAmount: number
}

interface ListOrdersDto {
  readonly items: readonly OrderDto[]
}

interface BatchCreateOrdersDto {
  readonly createdCount: number
  readonly items: readonly OrderDto[]
}

interface BatchDeleteOrdersDto {
  readonly requestedCount: number
  readonly deletedCount: number
  readonly missingIds: readonly string[]
}

export async function getOrders(): Promise<readonly Order[]> {
  const response = await requestJson<ListOrdersDto>('orders')
  return response.items.map(toOrder)
}

export async function postOrder(input: CreateOrderInput): Promise<Order> {
  const response = await requestJson<OrderDto>('orders', {
    method: 'POST',
    body: JSON.stringify(input),
  })
  return toOrder(response)
}

export async function getOrder(id: string): Promise<Order> {
  return toOrder(await requestJson<OrderDto>(`orders/${id}`))
}

export async function putOrder(
  id: string,
  input: UpdateOrderInput,
): Promise<Order> {
  const response = await requestJson<OrderDto>(`orders/${id}`, {
    method: 'PUT',
    body: JSON.stringify(input),
  })
  return toOrder(response)
}

export function deleteOrder(id: string): Promise<void> {
  return requestVoid(`orders/${id}`, { method: 'DELETE' })
}

export async function postOrdersBatch(
  input: BatchCreateOrdersInput,
): Promise<BatchCreateOrdersResult> {
  const response = await requestJson<BatchCreateOrdersDto>('orders/batch', {
    method: 'POST',
    body: JSON.stringify(input),
  })
  return {
    createdCount: response.createdCount,
    items: response.items.map(toOrder),
  }
}

export async function postOrdersBatchDelete(
  ids: readonly string[],
): Promise<BatchDeleteOrdersResult> {
  return requestJson<BatchDeleteOrdersDto>('orders/batch-delete', {
    method: 'POST',
    body: JSON.stringify({ ids }),
  })
}

function toOrder(dto: OrderDto): Order {
  return {
    id: dto.id,
    customerId: dto.customerId,
    customerName: dto.customerName,
    totalAmount: dto.totalAmount,
  }
}
