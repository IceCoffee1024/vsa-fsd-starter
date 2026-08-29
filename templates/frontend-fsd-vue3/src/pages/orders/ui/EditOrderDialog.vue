<script setup lang="ts">
import { computed, shallowRef, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { Save } from '@lucide/vue'
import type { Order } from '@/shared/api'
import { useOrderStore } from '../model/orders'
import { AppDialog } from '@/shared/ui'

const props = defineProps<{
  open: boolean
  order: Order | null
}>()

const emit = defineEmits<{
  close: []
  saved: [order: Order]
}>()

const orderStore = useOrderStore()
const { isMutating, mutationError } = storeToRefs(orderStore)
const totalAmount = shallowRef('')
const totalAmountError = shallowRef('')

const serverTotalAmountError = computed(
  () => mutationError.value?.problem?.errors?.totalAmount?.[0] || '',
)

watch(
  () => [props.open, props.order] as const,
  ([open, order]) => {
    if (open && order) {
      totalAmount.value = String(order.totalAmount)
      clearFeedback()
    }
  },
)

async function submit(): Promise<void> {
  if (!props.order) {
    return
  }
  clearFeedback()
  const parsedAmount = Number(totalAmount.value)
  if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
    totalAmountError.value = 'Enter an amount greater than zero.'
    return
  }

  try {
    const updated = await orderStore.update(props.order.id, {
      totalAmount: parsedAmount,
    })
    emit('saved', updated)
    emit('close')
  } catch {
    // The Store exposes the normalized error rendered below.
  }
}

function clearFeedback(): void {
  totalAmountError.value = ''
  orderStore.clearMutationError()
}
</script>

<template>
  <AppDialog
    :open="open"
    title="Edit order"
    :description="order ? `Order ${order.id.slice(0, 8)}` : ''"
    width="small"
    @close="emit('close')"
  >
    <form id="edit-order-form" class="edit-form" @submit.prevent="submit">
      <label class="dialog-field">
        <span class="dialog-field__label">Total amount</span>
        <input
          v-model="totalAmount"
          name="editTotalAmount"
          type="number"
          min="0.01"
          step="0.01"
          inputmode="decimal"
          :aria-invalid="Boolean(totalAmountError || serverTotalAmountError)"
          :disabled="isMutating"
          @input="clearFeedback"
        />
        <span
          v-if="totalAmountError || serverTotalAmountError"
          class="dialog-field__error"
        >
          {{ totalAmountError || serverTotalAmountError }}
        </span>
      </label>

      <div
        v-if="mutationError && !serverTotalAmountError"
        class="operation-error"
        role="alert"
      >
        <span>{{ mutationError.message }}</span>
        <span v-if="mutationError.problem?.traceId" class="operation-error__trace">
          Trace {{ mutationError.problem.traceId }}
        </span>
      </div>
    </form>

    <template #actions>
      <button class="button" type="button" :disabled="isMutating" @click="emit('close')">
        Cancel
      </button>
      <button
        class="button button--primary"
        type="submit"
        form="edit-order-form"
        :disabled="isMutating"
      >
        <Save :size="15" :stroke-width="2" aria-hidden="true" />
        {{ isMutating ? 'Saving...' : 'Save changes' }}
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.edit-form {
  display: grid;
  gap: 16px;
}
</style>
