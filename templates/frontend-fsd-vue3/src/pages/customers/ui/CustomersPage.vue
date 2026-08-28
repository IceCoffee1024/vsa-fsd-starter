<script setup lang="ts">
import { CreateCustomerForm } from '@/features/create-customer'
import { FindCustomerForm } from '@/features/find-customer'
</script>

<template>
  <div class="customers-page">
    <header class="customers-page__header">
      <div>
        <p class="customers-page__eyebrow">Operations</p>
        <h1>Customer registry</h1>
      </div>
      <p class="customers-page__summary">
        Register customers and resolve existing identities used by order records.
      </p>
    </header>

    <div class="customers-page__workspace">
      <CreateCustomerForm />
      <FindCustomerForm />
    </div>
  </div>
</template>

<style scoped>
.customers-page {
  max-width: 1280px;
  margin: 0 auto;
  padding: 38px 28px 64px;
}

.customers-page__header {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 32px;
  margin-bottom: 26px;
}

.customers-page__eyebrow {
  margin: 0 0 5px;
  color: #17656a;
  font-size: 11px;
  font-weight: 800;
  text-transform: uppercase;
}

.customers-page__header h1 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 28px;
  font-weight: 750;
}

.customers-page__summary {
  max-width: 480px;
  margin: 0;
  color: var(--color-text-muted);
  font-size: 13px;
  line-height: 1.6;
}

.customers-page__workspace {
  display: grid;
  grid-template-columns: minmax(280px, 360px) minmax(0, 1fr);
  gap: 22px;
  align-items: start;
}

@media (max-width: 860px) {
  .customers-page__header {
    display: grid;
  }

  .customers-page__workspace {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .customers-page {
    padding: 28px 18px 48px;
  }

  .customers-page__header h1 {
    font-size: 24px;
  }
}
</style>
