<script setup lang="ts">
import { shallowRef } from 'vue'
import { storeToRefs } from 'pinia'
import { Search } from '@lucide/vue'
import { CustomerSummary, useCustomerStore } from '@/entities/customer'
import { isGuid } from '@/shared/lib'

const customerStore = useCustomerStore()
const { isLookingUp, lookupCustomer, lookupError } = storeToRefs(customerStore)
const customerId = shallowRef('')
const customerIdError = shallowRef('')

async function submit(): Promise<void> {
  clearFeedback()
  if (!isGuid(customerId.value)) {
    customerIdError.value = 'Enter a valid customer UUID.'
    return
  }

  try {
    await customerStore.find(customerId.value.trim())
  } catch {
    // The Store exposes the normalized error rendered below.
  }
}

function clearFeedback(): void {
  customerIdError.value = ''
  customerStore.clearLookupFeedback()
}
</script>

<template>
  <section class="customer-lookup" aria-labelledby="find-customer-title">
    <div class="customer-lookup__heading">
      <h2 id="find-customer-title">Find customer</h2>
      <p>Resolve an existing customer before using its identifier in an order.</p>
    </div>

    <form class="customer-lookup__form" @submit.prevent="submit">
      <label class="field">
        <span class="field__label">Customer ID</span>
        <input
          v-model.trim="customerId"
          name="lookupCustomerId"
          type="text"
          autocomplete="off"
          placeholder="00000000-0000-0000-0000-000000000000"
          :aria-invalid="Boolean(customerIdError)"
          :disabled="isLookingUp"
          @input="clearFeedback"
        />
        <span v-if="customerIdError" class="field__error">
          {{ customerIdError }}
        </span>
      </label>

      <button class="secondary-button" type="submit" :disabled="isLookingUp">
        <Search :size="17" :stroke-width="2" aria-hidden="true" />
        {{ isLookingUp ? 'Looking up...' : 'Find customer' }}
      </button>
    </form>

    <div v-if="lookupError" class="lookup-error" role="alert">
      <span>{{ lookupError.message }}</span>
      <span v-if="lookupError.problem?.traceId" class="lookup-error__trace">
        Trace {{ lookupError.problem.traceId }}
      </span>
    </div>

    <CustomerSummary
      v-if="lookupCustomer"
      :customer="lookupCustomer"
      label="Customer found"
    />
  </section>
</template>

<style scoped>
.customer-lookup {
  display: grid;
  align-self: start;
  gap: 20px;
  padding: 24px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
}

.customer-lookup__heading h2 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 17px;
}

.customer-lookup__heading p {
  margin: 6px 0 0;
  color: var(--color-text-muted);
  font-size: 13px;
  line-height: 1.5;
}

.customer-lookup__form {
  display: grid;
  grid-template-columns: minmax(0, 1fr) auto;
  align-items: end;
  gap: 12px;
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
  font-family: var(--font-mono);
  font-size: 12px;
  outline: none;
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
}

.field__error {
  color: #a33232;
  font-family: var(--font-sans);
  font-size: 12px;
}

.secondary-button {
  display: inline-flex;
  min-height: 40px;
  align-items: center;
  justify-content: center;
  gap: 8px;
  padding: 0 15px;
  border: 1px solid #9cb4b7;
  border-radius: 5px;
  background: #f7fafb;
  color: #245b60;
  cursor: pointer;
  font: inherit;
  font-size: 13px;
  font-weight: 700;
}

.secondary-button:hover:not(:disabled) {
  background: #edf4f5;
}

.secondary-button:disabled {
  cursor: wait;
  opacity: 0.65;
}

.lookup-error {
  display: grid;
  gap: 4px;
  padding: 10px 11px;
  border-left: 3px solid #b64343;
  background: #fff3f2;
  color: #7e2929;
  font-size: 12px;
  line-height: 1.45;
}

.lookup-error__trace {
  font-family: var(--font-mono);
  font-size: 11px;
}

@media (max-width: 640px) {
  .customer-lookup__form {
    grid-template-columns: 1fr;
  }
}
</style>
