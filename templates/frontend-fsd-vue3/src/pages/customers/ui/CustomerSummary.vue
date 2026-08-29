<script setup lang="ts">
import { UserRound } from '@lucide/vue'
import type { Customer } from '@/shared/api'

defineProps<{
  customer: Customer
  label: string
}>()
</script>

<template>
  <div class="customer-summary" aria-live="polite">
    <span class="customer-summary__icon" aria-hidden="true">
      <UserRound :size="18" :stroke-width="2" />
    </span>
    <div class="customer-summary__content">
      <span class="customer-summary__label">{{ label }}</span>
      <strong>{{ customer.displayName }}</strong>
      <code>{{ customer.id }}</code>
    </div>
  </div>
</template>

<style scoped>
.customer-summary {
  display: flex;
  align-items: start;
  gap: 11px;
  padding-top: 16px;
  border-top: 1px solid var(--color-border);
}

.customer-summary__icon {
  display: inline-grid;
  width: 34px;
  height: 34px;
  flex: 0 0 34px;
  place-items: center;
  border: 1px solid #b8d1d3;
  border-radius: 5px;
  background: #eef6f6;
  color: #17656a;
}

.customer-summary__content {
  display: grid;
  min-width: 0;
  gap: 3px;
}

.customer-summary__label {
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 800;
  text-transform: uppercase;
}

.customer-summary strong {
  color: var(--color-text-strong);
  font-size: 14px;
}

.customer-summary code {
  overflow-wrap: anywhere;
  color: var(--color-text-muted);
  font-family: var(--font-mono);
  font-size: 11px;
}
</style>
