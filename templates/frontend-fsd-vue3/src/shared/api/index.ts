export {
  ApiError,
  configureHttpAuthentication,
  normalizeApiError,
  requestJson,
  requestVoid,
  type ProblemDetails,
} from './httpClient'
export {
  deleteOrder,
  getOrder,
  getOrders,
  postOrder,
  postOrdersBatch,
  postOrdersBatchDelete,
  putOrder,
  type BatchCreateOrdersInput,
  type BatchCreateOrdersResult,
  type BatchDeleteOrdersResult,
  type CreateOrderInput,
  type Order,
  type UpdateOrderInput,
} from './orders'
export {
  getCustomer,
  postCustomer,
  type CreateCustomerInput,
  type Customer,
} from './customers'
