<script setup lang="ts">
import { storeToRefs } from 'pinia'
import { Trash2 } from '@lucide/vue'
import type { Order } from '@/shared/api'
import { useOrderStore } from '../model/orders'
import { AppDialog } from '@/shared/ui'

const props = defineProps<{
  open: boolean
  order: Order | null
}>()

const emit = defineEmits<{
  close: []
  deleted: [id: string]
}>()

const orderStore = useOrderStore()
const { isMutating, mutationError } = storeToRefs(orderStore)

async function remove(): Promise<void> {
  if (!props.order) {
    return
  }
  orderStore.clearMutationError()
  try {
    await orderStore.remove(props.order.id)
    emit('deleted', props.order.id)
    emit('close')
  } catch {
    // The Store exposes the normalized error rendered below.
  }
}
</script>

<template>
  <AppDialog
    :open="open"
    title="Delete order"
    description="This action cannot be undone."
    width="small"
    @close="emit('close')"
  >
    <p class="delete-copy">
      Delete order <strong>{{ order?.id }}</strong> for
      <strong>{{ order?.customerName }}</strong>?
    </p>
    <div v-if="mutationError" class="operation-error" role="alert">
      <span>{{ mutationError.message }}</span>
      <span v-if="mutationError.problem?.traceId" class="operation-error__trace">
        Trace {{ mutationError.problem.traceId }}
      </span>
    </div>

    <template #actions>
      <button class="button" type="button" :disabled="isMutating" @click="emit('close')">
        Cancel
      </button>
      <button
        class="button button--danger"
        type="button"
        :disabled="isMutating"
        @click="remove"
      >
        <Trash2 :size="15" :stroke-width="2" aria-hidden="true" />
        {{ isMutating ? 'Deleting...' : 'Delete order' }}
      </button>
    </template>
  </AppDialog>
</template>

<style scoped>
.delete-copy {
  margin: 0 0 18px;
  color: var(--color-text);
  font-size: 13px;
  line-height: 1.6;
  overflow-wrap: anywhere;
}
</style>
