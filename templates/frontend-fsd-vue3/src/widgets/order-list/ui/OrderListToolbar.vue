<script setup lang="ts">
import { Plus, RefreshCw, Trash2, X } from '@lucide/vue'

defineProps<{
  recordCount: number
  selectedCount: number
  isLoading: boolean
}>()

const emit = defineEmits<{
  refresh: []
  batchCreate: []
  batchDelete: []
  clearSelection: []
}>()
</script>

<template>
  <div class="order-toolbar">
    <div class="order-toolbar__summary">
      <h2 id="order-list-title">Orders</h2>
      <p v-if="selectedCount">{{ selectedCount }} selected</p>
      <p v-else>{{ recordCount }} {{ recordCount === 1 ? 'record' : 'records' }}</p>
    </div>

    <div class="order-toolbar__actions">
      <button
        v-if="selectedCount"
        class="toolbar-button"
        type="button"
        @click="emit('clearSelection')"
      >
        <X :size="15" :stroke-width="2" aria-hidden="true" />
        Clear
      </button>
      <button
        v-if="selectedCount"
        class="toolbar-button toolbar-button--danger"
        type="button"
        @click="emit('batchDelete')"
      >
        <Trash2 :size="15" :stroke-width="2" aria-hidden="true" />
        Delete selected
      </button>
      <button class="toolbar-button" type="button" @click="emit('batchCreate')">
        <Plus :size="15" :stroke-width="2" aria-hidden="true" />
        Batch create
      </button>
      <button
        class="icon-button"
        type="button"
        title="Refresh orders"
        aria-label="Refresh orders"
        :disabled="isLoading"
        @click="emit('refresh')"
      >
        <RefreshCw
          :class="{ 'icon-button__icon--spinning': isLoading }"
          :size="17"
          :stroke-width="2"
          aria-hidden="true"
        />
      </button>
    </div>
  </div>
</template>

<style scoped>
.order-toolbar {
  display: flex;
  min-height: 70px;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  padding: 12px 16px;
  border-bottom: 1px solid var(--color-border);
}

.order-toolbar__summary h2 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 17px;
}

.order-toolbar__summary p {
  margin: 4px 0 0;
  color: var(--color-text-muted);
  font-size: 12px;
}

.order-toolbar__actions {
  display: flex;
  flex-wrap: wrap;
  justify-content: flex-end;
  gap: 7px;
}

.toolbar-button {
  display: inline-flex;
  min-height: 36px;
  align-items: center;
  gap: 6px;
  padding: 0 11px;
  border: 1px solid #cbd4d7;
  border-radius: 5px;
  background: #fff;
  color: #3d4b50;
  cursor: pointer;
  font: inherit;
  font-size: 12px;
  font-weight: 650;
}

.toolbar-button:hover {
  border-color: #8ea2a7;
  background: #f6f9f9;
}

.toolbar-button--danger {
  border-color: #d9b8b8;
  color: #9b3838;
}

.icon-button {
  display: inline-grid;
  width: 36px;
  height: 36px;
  flex: 0 0 36px;
  place-items: center;
  padding: 0;
  border: 1px solid #cbd4d7;
  border-radius: 5px;
  background: #fff;
  color: #3d4b50;
  cursor: pointer;
}

.icon-button:disabled {
  cursor: wait;
  opacity: 0.6;
}

.icon-button__icon--spinning {
  animation: spin 800ms linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 640px) {
  .order-toolbar {
    align-items: flex-start;
  }

  .order-toolbar__actions {
    max-width: 230px;
  }
}
</style>
