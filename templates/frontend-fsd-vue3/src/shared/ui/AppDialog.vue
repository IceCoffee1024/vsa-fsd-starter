<script setup lang="ts">
import { nextTick, useTemplateRef, watch } from 'vue'
import { X } from '@lucide/vue'

const props = withDefaults(
  defineProps<{
    open: boolean
    title: string
    description?: string
    width?: 'small' | 'medium' | 'large'
  }>(),
  {
    description: '',
    width: 'medium',
  },
)

const emit = defineEmits<{
  close: []
}>()

defineSlots<{
  default(): unknown
  actions(): unknown
}>()

const panel = useTemplateRef<HTMLElement>('panel')

watch(
  () => props.open,
  async (open) => {
    if (open) {
      await nextTick()
      panel.value?.focus()
    }
  },
)
</script>

<template>
  <div
    v-if="open"
    class="dialog-backdrop"
    role="presentation"
    @mousedown.self="emit('close')"
  >
    <section
      ref="panel"
      class="dialog-panel"
      :class="`dialog-panel--${width}`"
      role="dialog"
      aria-modal="true"
      :aria-label="title"
      tabindex="-1"
      @keydown.esc="emit('close')"
    >
      <header class="dialog-panel__header">
        <div>
          <h2>{{ title }}</h2>
          <p v-if="description">{{ description }}</p>
        </div>
        <button
          class="dialog-panel__close"
          type="button"
          title="Close"
          aria-label="Close"
          @click="emit('close')"
        >
          <X :size="18" :stroke-width="2" aria-hidden="true" />
        </button>
      </header>

      <div class="dialog-panel__body">
        <slot />
      </div>

      <footer v-if="$slots.actions" class="dialog-panel__actions">
        <slot name="actions" />
      </footer>
    </section>
  </div>
</template>

<style scoped>
.dialog-backdrop {
  position: fixed;
  z-index: 100;
  inset: 0;
  display: grid;
  place-items: center;
  overflow-y: auto;
  padding: 24px;
  background: rgb(24 36 41 / 42%);
}

.dialog-panel {
  width: 100%;
  max-height: calc(100vh - 48px);
  overflow: hidden;
  border: 1px solid var(--color-border);
  border-radius: 7px;
  background: var(--color-surface);
  box-shadow: 0 20px 50px rgb(24 36 41 / 24%);
  outline: none;
}

.dialog-panel--small {
  max-width: 430px;
}

.dialog-panel--medium {
  max-width: 560px;
}

.dialog-panel--large {
  max-width: 760px;
}

.dialog-panel__header {
  display: flex;
  min-height: 70px;
  align-items: center;
  justify-content: space-between;
  gap: 18px;
  padding: 15px 18px;
  border-bottom: 1px solid var(--color-border);
}

.dialog-panel__header h2 {
  margin: 0;
  color: var(--color-text-strong);
  font-size: 17px;
}

.dialog-panel__header p {
  margin: 4px 0 0;
  color: var(--color-text-muted);
  font-size: 12px;
}

.dialog-panel__close {
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

.dialog-panel__body {
  max-height: calc(100vh - 190px);
  overflow-y: auto;
  padding: 20px;
}

.dialog-panel__actions {
  display: flex;
  justify-content: flex-end;
  gap: 10px;
  padding: 13px 18px;
  border-top: 1px solid var(--color-border);
  background: #f7f9fa;
}

@media (max-width: 640px) {
  .dialog-backdrop {
    align-items: end;
    padding: 12px;
  }

  .dialog-panel {
    max-height: calc(100vh - 24px);
  }
}
</style>
