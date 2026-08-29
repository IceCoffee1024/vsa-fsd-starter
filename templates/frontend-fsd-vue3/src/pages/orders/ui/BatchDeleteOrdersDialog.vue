<script setup lang="ts">
import { shallowRef, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { Trash2 } from '@lucide/vue'
import type { BatchDeleteOrdersResult, Order } from '@/shared/api'
import { useOrderStore } from '../model/orders'
import { AppDialog } from '@/shared/ui'

const props = defineProps<{
  open: boolean
  orders: readonly Order[]
}>()

const emit = defineEmits<{
  close: []
  deleted: [result: BatchDeleteOrdersResult]
}>()

const orderStore = useOrderStore()
const { isMutating, mutationError } = storeToRefs(orderStore)
const result = shallowRef<BatchDeleteOrdersResult | null>(null)

watch(
  () => props.open,
  (open) => {
    if (open) {
      result.value = null
      orderStore.clearMutationError()
    }
  },
)

async function remove(): Promise<void> {
  orderStore.clearMutationError()
  try {
    result.value = await orderStore.removeMany(
      props.orders.map((order) => order.id),
    )
    emit('deleted', result.value)
  } catch {
    // The Store exposes the normalized error rendered below.
  }
}
</script>

<template>
  <AppDialog
    :open="open"
    title="Delete selected orders"
    description="Review the selected records before deleting them."
    width="medium"
    @close="emit('close')"
  >
    <div v-if="result" class="batch-result" aria-live="polite">
      <strong>{{ result.deletedCount }} of {{ result.requestedCount }} deleted</strong>
      <div v-if="result.missingIds.length">
        <span>Not found</span>
        <code v-for="id in result.missingIds" :key="id">{{ id }}</code>
      </div>
    </div>
    <template v-else>
      <p class="delete-copy">
        Delete {{ orders.length }} selected {{ orders.length === 1 ? 'order' : 'orders' }}?
        This action cannot be undone.
      </p>
      <ul class="selected-orders">
        <li v-for="order in orders" :key="order.id">
          <span>{{ order.customerName }}</span>
          <code>{{ order.id }}</code>
        </li>
      </ul>
      <div v-if="mutationError" class="operation-error" role="alert">
        <span>{{ mutationError.message }}</span>
        <span v-if="mutationError.problem?.traceId" class="operation-error__trace">
          Trace {{ mutationError.problem.traceId }}
        </span>
      </div>
    </template>

    <template #actions>
      <button v-if="result" class="button" type="button" @click="emit('close')">
        Done
      </button>
      <template v-else>
        <button class="button" type="button" :disabled="isMutating" @click="emit('close')">
          Cancel
        </button>
        <button
          class="button button--danger"
          type="button"
          :disabled="isMutating || orders.length === 0"
          @click="remove"
        >
          <Trash2 :size="15" :stroke-width="2" aria-hidden="true" />
          {{ isMutating ? 'Deleting...' : 'Delete selected' }}
        </button>
      </template>
    </template>
  </AppDialog>
</template>

<style scoped>
.delete-copy {
  margin: 0 0 14px;
  color: var(--color-text);
  font-size: 13px;
  line-height: 1.6;
}

.selected-orders {
  display: grid;
  max-height: 220px;
  gap: 0;
  overflow-y: auto;
  margin: 0 0 16px;
  padding: 0;
  border: 1px solid var(--color-border);
  list-style: none;
}

.selected-orders li {
  display: grid;
  gap: 3px;
  padding: 10px 12px;
  border-bottom: 1px solid var(--color-border-subtle);
}

.selected-orders li:last-child {
  border-bottom: 0;
}

.selected-orders span {
  color: var(--color-text-strong);
  font-size: 12px;
  font-weight: 650;
}

.selected-orders code,
.batch-result code {
  color: var(--color-text-muted);
  font-family: var(--font-mono);
  font-size: 11px;
  overflow-wrap: anywhere;
}

.batch-result {
  display: grid;
  gap: 14px;
  color: var(--color-text);
  font-size: 13px;
}

.batch-result div {
  display: grid;
  gap: 5px;
}
</style>
