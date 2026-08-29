<script setup lang="ts">
import { computed } from 'vue'
import { storeToRefs } from 'pinia'
import { ClipboardList } from '@lucide/vue'
import { useSessionStore } from '@/shared/auth'
import SessionControl from './SessionControl.vue'

const session = useSessionStore()
const { isAuthenticated } = storeToRefs(session)
const brandTarget = computed(() =>
  isAuthenticated.value ? '/orders' : '/sign-in',
)
</script>

<template>
  <div class="app-shell">
    <header class="app-header">
      <div class="app-header__inner">
        <RouterLink class="app-brand" :to="brandTarget">
          <span class="app-brand__mark" aria-hidden="true">
            <ClipboardList :size="20" :stroke-width="1.8" />
          </span>
          <span>Order Desk</span>
        </RouterLink>

        <div v-if="isAuthenticated" class="app-header__actions">
          <nav class="app-nav" aria-label="Primary navigation">
            <RouterLink to="/orders">Orders</RouterLink>
            <RouterLink to="/customers">Customers</RouterLink>
          </nav>
          <SessionControl />
        </div>
      </div>
    </header>

    <main class="app-main">
      <RouterView />
    </main>
  </div>
</template>

<style scoped>
.app-shell {
  min-height: 100vh;
}

.app-header {
  position: sticky;
  z-index: 10;
  top: 0;
  border-bottom: 1px solid var(--color-border);
  background: rgb(255 255 255 / 94%);
  backdrop-filter: blur(12px);
}

.app-header__inner {
  display: flex;
  min-height: 60px;
  max-width: 1280px;
  align-items: center;
  justify-content: space-between;
  margin: 0 auto;
  padding: 0 28px;
}

.app-brand {
  display: inline-flex;
  align-items: center;
  gap: 10px;
  color: var(--color-text-strong);
  font-size: 15px;
  font-weight: 700;
  text-decoration: none;
}

.app-brand__mark {
  display: inline-grid;
  width: 34px;
  height: 34px;
  place-items: center;
  border: 1px solid #c8d4d8;
  border-radius: 6px;
  background: #eef4f4;
  color: #17656a;
}

.app-header__actions {
  display: flex;
  align-items: center;
  gap: 22px;
}

.app-nav {
  display: flex;
  align-items: center;
  gap: 18px;
}

.app-nav a {
  display: inline-flex;
  min-height: 36px;
  align-items: center;
  border-bottom: 2px solid transparent;
  color: var(--color-text-muted);
  font-size: 14px;
  font-weight: 600;
  text-decoration: none;
}

.app-nav a.router-link-active {
  border-color: #17656a;
  color: #17656a;
}

.app-main {
  width: 100%;
}

@media (max-width: 640px) {
  .app-header__inner {
    padding: 0 14px;
  }

  .app-brand > span:last-child {
    display: none;
  }

  .app-header__actions {
    min-width: 0;
    gap: 10px;
  }

  .app-nav {
    gap: 8px;
  }

  .app-nav a {
    min-height: 32px;
    font-size: 12px;
  }
}
</style>
