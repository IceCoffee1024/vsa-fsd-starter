<script setup lang="ts">
import { computed, onMounted, shallowRef, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { OrderTable, useOrderStore, type Order } from '@/entities/order'
import { BatchCreateOrdersDialog } from '@/features/batch-create-orders'
import { BatchDeleteOrdersDialog } from '@/features/batch-delete-orders'
import { DeleteOrderDialog } from '@/features/delete-order'
import { EditOrderDialog } from '@/features/edit-order'
import { OrderDetailsDialog } from '@/features/view-order'
import OrderListToolbar from './OrderListToolbar.vue'

const orderStore = useOrderStore()
const { orders, listError, listStatus, isLoading } = storeToRefs(orderStore)
const { load } = orderStore

const selectedIds = shallowRef<readonly string[]>([])
const viewOrderId = shallowRef<string | null>(null)
const editOrderId = shallowRef<string | null>(null)
const deleteOrderId = shallowRef<string | null>(null)
const batchCreateOpen = shallowRef(false)
const batchDeleteOpen = shallowRef(false)
const feedback = shallowRef('')

const editOrder = computed(() => findOrder(editOrderId.value))
const deleteOrder = computed(() => findOrder(deleteOrderId.value))
const selectedOrders = computed(() => {
  const selected = new Set(selectedIds.value)
  return orders.value.filter((order) => selected.has(order.id))
})

onMounted(() => {
  if (listStatus.value === 'idle') {
    void load()
  }
})

watch(orders, (currentOrders) => {
  const existing = new Set(currentOrders.map((order) => order.id))
  selectedIds.value = selectedIds.value.filter((id) => existing.has(id))
})

function findOrder(id: string | null): Order | null {
  return id ? orders.value.find((order) => order.id === id) || null : null
}

function toggleSelection(id: string): void {
  const selected = new Set(selectedIds.value)
  if (selected.has(id)) {
    selected.delete(id)
  } else {
    selected.add(id)
  }
  selectedIds.value = [...selected]
}

function toggleAll(selected: boolean): void {
  selectedIds.value = selected ? orders.value.map((order) => order.id) : []
}

function showFeedback(message: string): void {
  feedback.value = message
}
</script>

<template>
  <section class="order-list" aria-labelledby="order-list-title">
    <OrderListToolbar
      :record-count="orders.length"
      :selected-count="selectedIds.length"
      :is-loading="isLoading"
      @refresh="load"
      @batch-create="batchCreateOpen = true"
      @batch-delete="batchDeleteOpen = true"
      @clear-selection="selectedIds = []"
    />

    <p v-if="feedback" class="order-list__feedback" aria-live="polite">
      {{ feedback }}
    </p>

    <div v-if="listError" class="order-list__error" role="alert">
      <div>
        <strong>Orders could not be loaded</strong>
        <p>{{ listError.message }}</p>
        <span v-if="listError.problem?.traceId">
          Trace {{ listError.problem.traceId }}
        </span>
      </div>
      <button type="button" @click="load">Retry</button>
    </div>

    <div v-else-if="isLoading && orders.length === 0" class="order-list__loading">
      Loading orders...
    </div>

    <OrderTable
      v-else
      :orders="orders"
      :selected-ids="selectedIds"
      @toggle="toggleSelection"
      @toggle-all="toggleAll"
      @view="viewOrderId = $event.id"
      @edit="editOrderId = $event.id"
      @delete="deleteOrderId = $event.id"
    />

    <OrderDetailsDialog
      :open="viewOrderId !== null"
      :order-id="viewOrderId"
      @close="viewOrderId = null"
    />
    <EditOrderDialog
      :open="editOrderId !== null"
      :order="editOrder"
      @close="editOrderId = null"
      @saved="showFeedback(`Order ${$event.id.slice(0, 8)} updated.`)"
    />
    <DeleteOrderDialog
      :open="deleteOrderId !== null"
      :order="deleteOrder"
      @close="deleteOrderId = null"
      @deleted="showFeedback(`Order ${$event.slice(0, 8)} deleted.`)"
    />
    <BatchCreateOrdersDialog
      :open="batchCreateOpen"
      @close="batchCreateOpen = false"
      @created="showFeedback(`${$event} orders created.`)"
    />
    <BatchDeleteOrdersDialog
      :open="batchDeleteOpen"
      :orders="selectedOrders"
      @close="batchDeleteOpen = false"
      @deleted="selectedIds = []"
    />
  </section>
</template>

<style scoped>
.order-list {
  min-width: 0;
  overflow: hidden;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
}

.order-list__feedback {
  margin: 0;
  padding: 9px 16px;
  border-bottom: 1px solid #cce3d4;
  background: #eff8f2;
  color: #235f3e;
  font-size: 12px;
}

.order-list__loading {
  display: grid;
  min-height: 220px;
  place-items: center;
  color: var(--color-text-muted);
  font-size: 13px;
}

.order-list__error {
  display: flex;
  min-height: 160px;
  align-items: center;
  justify-content: space-between;
  gap: 20px;
  margin: 18px;
  padding: 18px;
  border-left: 3px solid #b64343;
  background: #fff3f2;
  color: #7e2929;
}

.order-list__error strong {
  font-size: 13px;
}

.order-list__error p {
  margin: 5px 0;
  font-size: 12px;
}

.order-list__error span {
  font-family: var(--font-mono);
  font-size: 11px;
}

.order-list__error button {
  min-height: 34px;
  padding: 0 12px;
  border: 1px solid #b64343;
  border-radius: 5px;
  background: transparent;
  color: #7e2929;
  cursor: pointer;
  font: inherit;
  font-size: 12px;
  font-weight: 700;
}
</style>
