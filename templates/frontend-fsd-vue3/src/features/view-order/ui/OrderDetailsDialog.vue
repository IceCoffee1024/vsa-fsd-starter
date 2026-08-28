<script setup lang="ts">
import { shallowRef, watch } from 'vue'
import { getOrder, type Order } from '@/entities/order'
import { normalizeApiError, type ApiError } from '@/shared/api'
import { formatAmount } from '@/shared/lib'
import { AppDialog } from '@/shared/ui'

const props = defineProps<{
  open: boolean
  orderId: string | null
}>()

const emit = defineEmits<{
  close: []
}>()

const order = shallowRef<Order | null>(null)
const error = shallowRef<ApiError | null>(null)
const isLoading = shallowRef(false)

watch(
  () => [props.open, props.orderId] as const,
  async ([open, orderId]) => {
    if (!open || !orderId) {
      return
    }
    order.value = null
    error.value = null
    isLoading.value = true
    try {
      order.value = await getOrder(orderId)
    } catch (requestError) {
      error.value = normalizeApiError(requestError)
    } finally {
      isLoading.value = false
    }
  },
)
</script>

<template>
  <AppDialog
    :open="open"
    title="Order details"
    width="medium"
    @close="emit('close')"
  >
    <div v-if="isLoading" class="details-state">Loading order...</div>
    <div v-else-if="error" class="operation-error" role="alert">
      <strong>Order could not be loaded</strong>
      <span>{{ error.message }}</span>
      <span v-if="error.problem?.traceId" class="operation-error__trace">
        Trace {{ error.problem.traceId }}
      </span>
    </div>
    <dl v-else-if="order" class="order-details">
      <div>
        <dt>Order ID</dt>
        <dd class="order-details__id">{{ order.id }}</dd>
      </div>
      <div>
        <dt>Customer</dt>
        <dd>{{ order.customerName }}</dd>
      </div>
      <div>
        <dt>Customer ID</dt>
        <dd class="order-details__id">{{ order.customerId }}</dd>
      </div>
      <div>
        <dt>Total amount</dt>
        <dd>{{ formatAmount(order.totalAmount) }}</dd>
      </div>
    </dl>

    <template #actions>
      <button class="button" type="button" @click="emit('close')">Close</button>
    </template>
  </AppDialog>
</template>

<style scoped>
.details-state {
  display: grid;
  min-height: 150px;
  place-items: center;
  color: var(--color-text-muted);
  font-size: 13px;
}

.order-details {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 18px 24px;
  margin: 0;
}

.order-details div {
  min-width: 0;
}

.order-details dt {
  margin-bottom: 5px;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.order-details dd {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 13px;
  overflow-wrap: anywhere;
}

.order-details__id {
  font-family: var(--font-mono);
  font-size: 12px !important;
}

@media (max-width: 520px) {
  .order-details {
    grid-template-columns: 1fr;
  }
}
</style>
