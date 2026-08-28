<script setup lang="ts">
import { computed, shallowRef } from 'vue'
import { storeToRefs } from 'pinia'
import { UserPlus } from '@lucide/vue'
import { CustomerSummary, useCustomerStore } from '@/entities/customer'

const customerStore = useCustomerStore()
const { createdCustomer, createError, isCreating } = storeToRefs(customerStore)
const displayName = shallowRef('')
const displayNameError = shallowRef('')

const serverDisplayNameError = computed(
  () => createError.value?.problem?.errors?.displayName?.[0] || '',
)

async function submit(): Promise<void> {
  clearFeedback()
  const normalizedName = displayName.value.trim()
  if (!normalizedName) {
    displayNameError.value = 'Enter a display name.'
  } else if (normalizedName.length > 100) {
    displayNameError.value = 'Use no more than 100 characters.'
  }
  if (displayNameError.value) {
    return
  }

  try {
    await customerStore.create({ displayName: normalizedName })
    displayName.value = ''
  } catch {
    // The Store exposes field and request errors rendered below.
  }
}

function clearFeedback(): void {
  displayNameError.value = ''
  customerStore.clearCreateFeedback()
}
</script>

<template>
  <section class="customer-form" aria-labelledby="create-customer-title">
    <div class="customer-form__heading">
      <h2 id="create-customer-title">New customer</h2>
      <p>Create an identity that can be referenced by future orders.</p>
    </div>

    <form class="customer-form__body" @submit.prevent="submit">
      <label class="field">
        <span class="field__label">Display name</span>
        <input
          v-model="displayName"
          name="displayName"
          type="text"
          autocomplete="name"
          maxlength="100"
          placeholder="Ada Lovelace"
          :aria-invalid="Boolean(displayNameError || serverDisplayNameError)"
          :disabled="isCreating"
          @input="clearFeedback"
        />
        <span
          v-if="displayNameError || serverDisplayNameError"
          class="field__error"
        >
          {{ displayNameError || serverDisplayNameError }}
        </span>
      </label>

      <div
        v-if="createError && !serverDisplayNameError"
        class="form-message form-message--error"
        role="alert"
      >
        <span>{{ createError.message }}</span>
        <span v-if="createError.problem?.traceId" class="form-message__trace">
          Trace {{ createError.problem.traceId }}
        </span>
      </div>

      <button class="primary-button" type="submit" :disabled="isCreating">
        <UserPlus :size="17" :stroke-width="2" aria-hidden="true" />
        {{ isCreating ? 'Creating...' : 'Create customer' }}
      </button>
    </form>

    <CustomerSummary
      v-if="createdCustomer"
      :customer="createdCustomer"
      label="Customer created"
    />
  </section>
</template>

<style scoped>
.customer-form {
  display: grid;
  align-self: start;
  gap: 20px;
  padding: 24px;
  border: 1px solid var(--color-border);
  border-radius: 6px;
  background: var(--color-surface);
}

.customer-form__heading h2 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 17px;
}

.customer-form__heading p {
  margin: 6px 0 0;
  color: var(--color-text-muted);
  font-size: 13px;
  line-height: 1.5;
}

.customer-form__body,
.field {
  display: grid;
}

.customer-form__body {
  gap: 18px;
}

.field {
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
  font-size: 12px;
}

.form-message {
  display: grid;
  gap: 4px;
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
