<script setup lang="ts">
import { CreateOrderForm } from '@/features/create-order'
import { OrderListWidget } from '@/widgets/order-list'
</script>

<template>
  <div class="orders-page">
    <header class="orders-page__header">
      <div>
        <p class="orders-page__eyebrow">Operations</p>
        <h1>Order management</h1>
      </div>
      <p class="orders-page__summary">
        Review current orders and create a new record for an existing customer.
      </p>
    </header>

    <div class="orders-page__workspace">
      <CreateOrderForm />
      <OrderListWidget />
    </div>
  </div>
</template>

<style scoped>
.orders-page {
  max-width: 1280px;
  margin: 0 auto;
  padding: 38px 28px 64px;
}

.orders-page__header {
  display: flex;
  align-items: end;
  justify-content: space-between;
  gap: 32px;
  margin-bottom: 26px;
}

.orders-page__eyebrow {
  margin: 0 0 5px;
  color: #17656a;
  font-size: 11px;
  font-weight: 800;
  letter-spacing: 0;
  text-transform: uppercase;
}

.orders-page__header h1 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 28px;
  font-weight: 750;
  letter-spacing: 0;
}

.orders-page__summary {
  max-width: 480px;
  margin: 0;
  color: var(--color-text-muted);
  font-size: 13px;
  line-height: 1.6;
}

.orders-page__workspace {
  display: grid;
  grid-template-columns: minmax(250px, 320px) minmax(0, 1fr);
  gap: 22px;
  align-items: start;
}

@media (max-width: 860px) {
  .orders-page__header {
    display: grid;
  }

  .orders-page__workspace {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 640px) {
  .orders-page {
    padding: 28px 18px 48px;
  }

  .orders-page__header h1 {
    font-size: 24px;
  }
}
</style>
