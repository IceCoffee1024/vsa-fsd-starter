<script setup lang="ts">
import { computed } from 'vue'
import { Eye, Pencil, Trash2, PackageOpen } from '@lucide/vue'
import { formatAmount } from '@/shared/lib'
import type { Order } from '@/shared/api'

const props = defineProps<{
  orders: readonly Order[]
  selectedIds: readonly string[]
}>()

const emit = defineEmits<{
  view: [order: Order]
  edit: [order: Order]
  delete: [order: Order]
  toggle: [id: string]
  toggleAll: [selected: boolean]
}>()

const selectedIdSet = computed(() => new Set(props.selectedIds))
const allSelected = computed(
  () => props.orders.length > 0 && props.selectedIds.length === props.orders.length,
)
</script>

<template>
  <div v-if="orders.length === 0" class="empty-state">
    <PackageOpen :size="28" :stroke-width="1.5" aria-hidden="true" />
    <p>No orders yet</p>
  </div>

  <div v-else class="table-scroll">
    <table class="order-table">
      <thead>
        <tr>
          <th class="order-table__select" scope="col">
            <input
              type="checkbox"
              aria-label="Select all orders"
              :checked="allSelected"
              @change="emit('toggleAll', ($event.target as HTMLInputElement).checked)"
            />
          </th>
          <th scope="col">Order ID</th>
          <th scope="col">Customer</th>
          <th scope="col">Customer ID</th>
          <th class="order-table__amount" scope="col">Total</th>
          <th class="order-table__actions" scope="col">Actions</th>
        </tr>
      </thead>
      <tbody>
        <tr
          v-for="order in orders"
          :key="order.id"
          :class="{ 'order-table__row--selected': selectedIdSet.has(order.id) }"
        >
          <td class="order-table__select">
            <input
              type="checkbox"
              :aria-label="`Select order ${order.id}`"
              :checked="selectedIdSet.has(order.id)"
              @change="emit('toggle', order.id)"
            />
          </td>
          <td class="order-table__id">{{ order.id }}</td>
          <td class="order-table__customer">{{ order.customerName }}</td>
          <td class="order-table__id">{{ order.customerId }}</td>
          <td class="order-table__amount">{{ formatAmount(order.totalAmount) }}</td>
          <td class="order-table__actions">
            <div class="row-actions">
              <button
                type="button"
                title="View order"
                :aria-label="`View order ${order.id}`"
                @click="emit('view', order)"
              >
                <Eye :size="15" :stroke-width="2" aria-hidden="true" />
              </button>
              <button
                type="button"
                title="Edit order"
                :aria-label="`Edit order ${order.id}`"
                @click="emit('edit', order)"
              >
                <Pencil :size="15" :stroke-width="2" aria-hidden="true" />
              </button>
              <button
                class="row-actions__danger"
                type="button"
                title="Delete order"
                :aria-label="`Delete order ${order.id}`"
                @click="emit('delete', order)"
              >
                <Trash2 :size="15" :stroke-width="2" aria-hidden="true" />
              </button>
            </div>
          </td>
        </tr>
      </tbody>
    </table>
  </div>
</template>

<style scoped>
.table-scroll {
  overflow-x: auto;
}

.order-table {
  width: 100%;
  min-width: 900px;
  border-collapse: collapse;
}

.order-table th,
.order-table td {
  padding: 12px 14px;
  border-bottom: 1px solid var(--color-border-subtle);
  text-align: left;
  vertical-align: middle;
}

.order-table th {
  background: #f7f9fa;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
  text-transform: uppercase;
}

.order-table td {
  color: var(--color-text);
  font-size: 13px;
}

.order-table tbody tr:hover,
.order-table__row--selected {
  background: #f0f7f7;
}

.order-table__select {
  width: 42px;
  text-align: center !important;
}

.order-table__select input {
  width: 15px;
  height: 15px;
  accent-color: #17656a;
}

.order-table__id {
  color: #56646a;
  font-family: var(--font-mono);
  font-size: 12px !important;
}

.order-table__customer {
  color: var(--color-text-strong);
  font-weight: 600;
}

.order-table__amount {
  text-align: right !important;
  font-variant-numeric: tabular-nums;
}

.order-table__actions {
  width: 128px;
  text-align: right !important;
}

.row-actions {
  display: inline-flex;
  gap: 5px;
}

.row-actions button {
  display: inline-grid;
  width: 30px;
  height: 30px;
  place-items: center;
  padding: 0;
  border: 1px solid #ccd6d9;
  border-radius: 4px;
  background: #fff;
  color: #46565c;
  cursor: pointer;
}

.row-actions button:hover {
  border-color: #8ea2a7;
  background: #f6f9f9;
}

.row-actions__danger {
  color: #9b3838 !important;
}

.empty-state {
  display: grid;
  min-height: 220px;
  place-items: center;
  align-content: center;
  gap: 10px;
  color: #8a979c;
}

.empty-state p {
  margin: 0;
  font-size: 14px;
}
</style>
