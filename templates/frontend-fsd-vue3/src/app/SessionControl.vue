<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { LogOut, RefreshCw, UserRound } from '@lucide/vue'
import { useRouter } from 'vue-router'
import { useSessionStore } from '@/shared/auth'

const router = useRouter()
const session = useSessionStore()
const { isRefreshing, method, username } = storeToRefs(session)

async function refresh(): Promise<void> {
  try {
    await session.refreshOAuth()
  } catch {
    await router.replace({ name: 'sign-in' })
  }
}

async function signOut(): Promise<void> {
  session.signOut()
  await router.replace({ name: 'sign-in' })
}
</script>

<template>
  <div class="session-control">
    <span class="session-control__identity">
      <UserRound :size="15" :stroke-width="2" aria-hidden="true" />
      <span>{{ username }}</span>
      <small>{{ method === 'oauth' ? 'OAuth' : 'Basic' }}</small>
    </span>
    <button
      v-if="method === 'oauth'"
      class="icon-button"
      type="button"
      title="Refresh OAuth token"
      aria-label="Refresh OAuth token"
      :disabled="isRefreshing"
      @click="refresh"
    >
      <RefreshCw
        :class="{ 'icon-button__spinning': isRefreshing }"
        :size="16"
        :stroke-width="2"
        aria-hidden="true"
      />
    </button>
    <button
      class="icon-button"
      type="button"
      title="Sign out"
      aria-label="Sign out"
      @click="signOut"
    >
      <LogOut :size="16" :stroke-width="2" aria-hidden="true" />
    </button>
  </div>
</template>

<style scoped>
.session-control {
  display: flex;
  align-items: center;
  gap: 7px;
}

.session-control__identity {
  display: inline-flex;
  min-width: 0;
  align-items: center;
  gap: 6px;
  color: var(--color-text);
  font-size: 12px;
  font-weight: 650;
}

.session-control__identity small {
  padding-left: 6px;
  border-left: 1px solid var(--color-border);
  color: var(--color-text-muted);
  font-size: 10px;
  font-weight: 600;
}

.icon-button {
  display: inline-grid;
  width: 34px;
  height: 34px;
  flex: 0 0 34px;
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

.icon-button__spinning {
  animation: spin 800ms linear infinite;
}

@keyframes spin {
  to {
    transform: rotate(360deg);
  }
}

@media (max-width: 640px) {
  .session-control__identity small {
    display: none;
  }
}
</style>
