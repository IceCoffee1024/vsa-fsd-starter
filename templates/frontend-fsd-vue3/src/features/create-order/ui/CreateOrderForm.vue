<script setup lang="ts">
import { computed, shallowRef } from 'vue'
import { storeToRefs } from 'pinia'
import { Plus } from '@lucide/vue'
import { useOrderStore } from '@/entities/order'
import { isGuid } from '@/shared/lib'

const orderStore = useOrderStore()
const { isMutating, mutationError } = storeToRefs(orderStore)

const customerId = shallowRef('')
const totalAmount = shallowRef('')
const customerIdError = shallowRef('')
const totalAmountError = shallowRef('')
const successMessage = shallowRef('')

const serverCustomerIdError = computed(
  () => mutationError.value?.problem?.errors?.customerId?.[0] || '',
)
const serverTotalAmountError = computed(
  () => mutationError.value?.problem?.errors?.totalAmount?.[0] || '',
)

async function submit(): Promise<void> {
  clearFeedback()

  const parsedAmount = Number(totalAmount.value)
  if (!isGuid(customerId.value)) {
    customerIdError.value = 'Enter a valid customer UUID.'
  }
  if (!Number.isFinite(parsedAmount) || parsedAmount <= 0) {
    totalAmountError.value = 'Enter an amount greater than zero.'
  }
  if (customerIdError.value || totalAmountError.value) {
    return
  }

  try {
    const created = await orderStore.create({
      customerId: customerId.value.trim(),
      totalAmount: parsedAmount,
    })
    customerId.value = ''
    totalAmount.value = ''
    successMessage.value = `Order ${created.id.slice(0, 8)} created.`
  } catch {
    return
  }
}

function clearFeedback(): void {
  customerIdError.value = ''
  totalAmountError.value = ''
  successMessage.value = ''
  orderStore.clearMutationError()
}
</script>

<template>
  <section class="create-order" aria-labelledby="create-order-title">
    <div class="create-order__heading">
      <h2 id="create-order-title">New order</h2>
      <p>Create an order for an existing customer.</p>
    </div>

    <form class="create-order__form" @submit.prevent="submit">
      <label class="field">
        <span class="field__label">Customer ID</span>
        <input
          v-model.trim="customerId"
          name="customerId"
          type="text"
          autocomplete="off"
          placeholder="00000000-0000-0000-0000-000000000000"
          :aria-invalid="Boolean(customerIdError || serverCustomerIdError)"
          :disabled="isMutating"
          @input="clearFeedback"
        />
        <span
          v-if="customerIdError || serverCustomerIdError"
          class="field__error"
        >
          {{ customerIdError || serverCustomerIdError }}
        </span>
      </label>

      <label class="field">
        <span class="field__label">Total amount</span>
        <input
          v-model="totalAmount"
          name="totalAmount"
          type="number"
          min="0.01"
          step="0.01"
          inputmode="decimal"
          placeholder="0.00"
          :aria-invalid="Boolean(totalAmountError || serverTotalAmountError)"
          :disabled="isMutating"
          @input="clearFeedback"
        />
        <span
          v-if="totalAmountError || serverTotalAmountError"
          class="field__error"
        >
          {{ totalAmountError || serverTotalAmountError }}
        </span>
      </label>

      <div
        v-if="mutationError && !serverCustomerIdError && !serverTotalAmountError"
        class="form-message form-message--error"
        role="alert"
      >
        <span>{{ mutationError.message }}</span>
        <span v-if="mutationError.problem?.traceId" class="form-message__trace">
          Trace {{ mutationError.problem.traceId }}
        </span>
      </div>

      <p
        v-if="successMessage"
        class="form-message form-message--success"
        aria-live="polite"
      >
        {{ successMessage }}
      </p>

      <button class="primary-button" type="submit" :disabled="isMutating">
        <Plus :size="17" :stroke-width="2" aria-hidden="true" />
        <span>{{ isMutating ? 'Creating...' : 'Create order' }}</span>
      </button>
    </form>
  </section>
</template>

<style scoped>
.create-order {
  align-self: start;
  padding: 24px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
}

.create-order__heading {
  margin-bottom: 22px;
}

.create-order__heading h2 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 17px;
  font-weight: 700;
}

.create-order__heading p {
  margin: 6px 0 0;
  color: var(--color-text-muted);
  font-size: 13px;
  line-height: 1.5;
}

.create-order__form {
  display: grid;
  gap: 18px;
}

.field {
  display: grid;
  gap: 7px;
}

.field__label {
  color: var(--color-text);
  font-size: 12px;
  font-weight: 700;
}

.field input {
  width: 100%;
  min-height: 40px;
  padding: 0 11px;
  border: 1px solid #cbd4d7;
  border-radius: 5px;
  background: #fff;
  color: var(--color-text-strong);
  font: inherit;
  font-size: 13px;
  outline: none;
  transition:
    border-color 140ms ease,
    box-shadow 140ms ease;
}

.field input:focus {
  border-color: #268087;
  box-shadow: 0 0 0 3px rgb(38 128 135 / 14%);
}

.field input[aria-invalid='true'] {
  border-color: #b64343;
}

.field input:disabled {
  background: #f1f3f4;
  color: #7c888c;
}

.field__error {
  color: #a33232;
  font-size: 12px;
  line-height: 1.4;
}

.form-message {
  display: grid;
  gap: 4px;
  margin: 0;
  padding: 10px 11px;
  border-left: 3px solid;
  font-size: 12px;
  line-height: 1.45;
}

.form-message--error {
  border-color: #b64343;
  background: #fff3f2;
  color: #7e2929;
}

.form-message--success {
  border-color: #2f7b52;
  background: #eff8f2;
  color: #235f3e;
}

.form-message__trace {
  font-family: var(--font-mono);
  font-size: 11px;
}

.primary-button {
  display: inline-flex;
  min-height: 40px;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0 15px;
  border: 1px solid #17656a;
  border-radius: 5px;
  background: #17656a;
  color: #fff;
  cursor: pointer;
  font: inherit;
  font-size: 13px;
  font-weight: 700;
}

.primary-button:hover:not(:disabled) {
  background: #12575b;
}

.primary-button:disabled {
  cursor: wait;
  opacity: 0.65;
}
</style>
