import { computed, shallowRef } from 'vue'
import { defineStore } from 'pinia'
import {
  getCustomer,
  normalizeApiError,
  postCustomer,
  type ApiError,
  type CreateCustomerInput,
  type Customer,
} from '@/shared/api'

type CustomerRequestStatus = 'idle' | 'pending' | 'success' | 'error'

export const useCustomerStore = defineStore('customer', () => {
  const createdCustomer = shallowRef<Customer | null>(null)
  const lookupCustomer = shallowRef<Customer | null>(null)
  const createStatus = shallowRef<CustomerRequestStatus>('idle')
  const lookupStatus = shallowRef<CustomerRequestStatus>('idle')
  const createError = shallowRef<ApiError | null>(null)
  const lookupError = shallowRef<ApiError | null>(null)

  const isCreating = computed(() => createStatus.value === 'pending')
  const isLookingUp = computed(() => lookupStatus.value === 'pending')

  async function create(input: CreateCustomerInput): Promise<Customer> {
    createStatus.value = 'pending'
    createError.value = null
    createdCustomer.value = null

    try {
      const customer = await postCustomer(input)
      createdCustomer.value = customer
      createStatus.value = 'success'
      return customer
    } catch (error) {
      createError.value = normalizeApiError(error)
      createStatus.value = 'error'
      throw createError.value
    }
  }

  async function find(id: string): Promise<Customer> {
    lookupStatus.value = 'pending'
    lookupError.value = null
    lookupCustomer.value = null

    try {
      const customer = await getCustomer(id)
      lookupCustomer.value = customer
      lookupStatus.value = 'success'
      return customer
    } catch (error) {
      lookupError.value = normalizeApiError(error)
      lookupStatus.value = 'error'
      throw lookupError.value
    }
  }

  function clearCreateFeedback(): void {
    createdCustomer.value = null
    createError.value = null
    if (createStatus.value !== 'pending') {
      createStatus.value = 'idle'
    }
  }

  function clearLookupFeedback(): void {
    lookupCustomer.value = null
    lookupError.value = null
    if (lookupStatus.value !== 'pending') {
      lookupStatus.value = 'idle'
    }
  }

  return {
    createdCustomer,
    lookupCustomer,
    createStatus,
    lookupStatus,
    createError,
    lookupError,
    isCreating,
    isLookingUp,
    create,
    find,
    clearCreateFeedback,
    clearLookupFeedback,
  }
})
