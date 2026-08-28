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

export type RequestStatus = 'idle' | 'pending' | 'success' | 'error'
