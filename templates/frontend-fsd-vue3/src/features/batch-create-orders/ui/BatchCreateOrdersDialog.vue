<script setup lang="ts">
import { computed, ref, shallowRef, watch } from 'vue'
import { storeToRefs } from 'pinia'
import { Plus, Trash2 } from '@lucide/vue'
import { useOrderStore } from '@/entities/order'
import { isGuid } from '@/shared/lib'
import { AppDialog } from '@/shared/ui'

interface DraftRow {
  readonly key: number
  customerId: string
  totalAmount: string
  customerIdError: string
  totalAmountError: string
}

const props = defineProps<{
  open: boolean
}>()

const emit = defineEmits<{
  close: []
  created: [count: number]
}>()

const orderStore = useOrderStore()
const { isMutating, mutationError } = storeToRefs(orderStore)
const rows = ref<DraftRow[]>([])
const nextKey = shallowRef(1)

const collectionError = computed(
  () => mutationError.value?.problem?.errors?.orders?.[0] || '',
)

watch(
  () => props.open,
  (open) => {
    if (open) {
      rows.value = [createRow()]
      orderStore.clearMutationError()
    }
  },
)

function createRow(): DraftRow {
  return {
    key: nextKey.value++,
    customerId: '',
    totalAmount: '',
    customerIdError: '',
    totalAmountError: '',
  }
}

function addRow(): void {
  if (rows.value.length < 100) {
    rows.value.push(createRow())
  }
}

function removeRow(key: number): void {
  if (rows.value.length > 1) {
    rows.value = rows.value.filter((row) => row.key !== key)
  }
}

function clearRowFeedback(row: DraftRow): void {
  row.customerIdError = ''
  row.totalAmountError = ''
  orderStore.clearMutationError()
}

function serverError(index: number, field: string): string {
  return (
    mutationError.value?.problem?.errors?.[`orders[${index}].${field}`]?.[0] ||
    ''
  )
}

async function submit(): Promise<void> {
  orderStore.clearMutationError()
  let hasErrors = false
  const orders = rows.value.map((row) => {
    clearRowFeedback(row)
    const amount = Number(row.totalAmount)
    if (!isGuid(row.customerId)) {
      row.customerIdError = 'Enter a valid customer UUID.'
      hasErrors = true
    }
    if (!Number.isFinite(amount) || amount <= 0) {
      row.totalAmountError = 'Enter an amount greater than zero.'
      hasErrors = true
    }
    return {
      customerId: row.customerId.trim(),
      totalAmount: amount,
    }
  })

  if (hasErrors) {
    return
  }

  try {
    const result = await orderStore.createMany({ orders })
    emit('created', result.createdCount)
    emit('close')
  } catch {
    // The Store exposes field and request errors rendered below.
  }
}
</script>

<template>
  <AppDialog
    :open="open"
    title="Batch create orders"
    description="Create between 1 and 100 orders."
    width="large"
    @close="emit('close')"
  >
    <form id="batch-create-form" class="batch-form" @submit.prevent="submit">
      <div class="batch-form__header">
        <span>{{ rows.length }} {{ rows.length === 1 ? 'order' : 'orders' }}</span>
        <button
          class="button"
          type="button"
          :disabled="rows.length >= 100 || isMutating"
          @click="addRow"
        >
          <Plus :size="15" :stroke-width="2" aria-hidden="true" />
          Add row
        </button>
      </div>

      <div class="batch-form__rows">
        <fieldset v-for="(row, index) in rows" :key="row.key" class="batch-row">
          <legend>Order {{ index + 1 }}</legend>
          <label class="dialog-field">
            <span class="dialog-field__label">Customer ID</span>
            <input
              v-model="row.customerId"
              :name="`orders[${index}].customerId`"
              type="text"
              autocomplete="off"
              placeholder="00000000-0000-0000-0000-000000000000"
              :aria-invalid="Boolean(row.customerIdError || serverError(index, 'customerId'))"
              :disabled="isMutating"
              @input="clearRowFeedback(row)"
            />
            <span
              v-if="row.customerIdError || serverError(index, 'customerId')"
              class="dialog-field__error"
            >
              {{ row.customerIdError || serverError(index, 'customerId') }}
            </span>
          </label>
          <label class="dialog-field">
            <span class="dialog-field__label">Total amount</span>
            <input
              v-model="row.totalAmount"
              :name="`orders[${index}].totalAmount`"
              type="number"
              min="0.01"
              step="0.01"
              inputmode="decimal"
              :aria-invalid="Boolean(row.totalAmountError || serverError(index, 'totalAmount'))"
              :disabled="isMutating"
              @input="clearRowFeedback(row)"
            />
            <span
              v-if="row.totalAmountError || serverError(index, 'totalAmount')"
              class="dialog-field__error"
            >
              {{ row.totalAmountError || serverError(index, 'totalAmount') }}
            </span>
          </label>
          <button
            class="batch-row__remove"
            type="button"
            title="Remove row"
            aria-label="Remove row"
            :disabled="rows.length === 1 || isMutating"
            @click="removeRow(row.key)"
          >
            <Trash2 :size="16" :stroke-width="2" aria-hidden="true" />
          </button>
        </fieldset>
      </div>

      <div v-if="collectionError" class="operation-error" role="alert">
        <span>{{ collectionError }}</span>
      </div>
      <div v-else-if="mutationError" class="operation-error" role="alert">
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
        form="batch-create-form"
        :disabled="isMutating"
      >
        <Plus :size="15" :stroke-width="2" aria-hidden="true" />
        {{ isMutating ? 'Creating...' : 'Create orders' }}
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.batch-form {
  display: grid;
  gap: 16px;
}

.batch-form__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  color: var(--color-text-muted);
  font-size: 12px;
}

.batch-form__rows {
  display: grid;
  gap: 12px;
}

.batch-row {
  position: relative;
  display: grid;
  grid-template-columns: minmax(0, 1fr) minmax(130px, 180px) 36px;
  gap: 12px;
  margin: 0;
  padding: 18px 14px 14px;
  border: 1px solid var(--color-border);
  border-radius: 5px;
}

.batch-row legend {
  padding: 0 5px;
  color: var(--color-text-muted);
  font-size: 11px;
  font-weight: 700;
}

.batch-row__remove {
  display: inline-grid;
  width: 36px;
  height: 36px;
  align-self: end;
  place-items: center;
  padding: 0;
  border: 1px solid #d9b8b8;
  border-radius: 5px;
  background: #fff;
  color: #9b3838;
  cursor: pointer;
}

.batch-row__remove:disabled {
  cursor: not-allowed;
  opacity: 0.4;
}

@media (max-width: 640px) {
  .batch-row {
    grid-template-columns: minmax(0, 1fr) 36px;
  }

  .batch-row .dialog-field:first-of-type {
    grid-column: 1 / -1;
  }
}
</style>
