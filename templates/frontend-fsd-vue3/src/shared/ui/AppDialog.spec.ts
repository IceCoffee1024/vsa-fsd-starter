import { flushPromises, mount, type VueWrapper } from '@vue/test-utils'
import { afterEach, describe, expect, it } from 'vitest'
import AppDialog from './AppDialog.vue'

const mountedWrappers: VueWrapper[] = []

afterEach(() => {
  for (const wrapper of mountedWrappers.splice(0)) {
    wrapper.unmount()
  }
  document.body.replaceChildren()
  document.body.style.overflow = ''
})

describe('AppDialog', () => {
  it('locks scrolling and restores focus when closed', async () => {
    const trigger = document.createElement('button')
    trigger.textContent = 'Open dialog'
    document.body.append(trigger)
    trigger.focus()

    const wrapper = mountDialog(false)
    await wrapper.setProps({ open: true })
    await flushPromises()

    expect(document.activeElement).toBe(wrapper.get('[role="dialog"]').element)
    expect(document.body.style.overflow).toBe('hidden')

    await wrapper.setProps({ open: false })
    await flushPromises()

    expect(document.activeElement).toBe(trigger)
    expect(document.body.style.overflow).toBe('')
  })

  it('keeps Tab navigation inside the dialog', async () => {
    const wrapper = mountDialog(true)
    await flushPromises()
    const panel = wrapper.get('[role="dialog"]')
    const closeButton = wrapper.get('[aria-label="Close"]')
    const saveButton = wrapper.get('[data-testid="save"]')
    const closeButtonElement = closeButton.element as HTMLElement
    const saveButtonElement = saveButton.element as HTMLElement

    await panel.trigger('keydown', { key: 'Tab', shiftKey: true })
    expect(document.activeElement).toBe(saveButtonElement)

    saveButtonElement.focus()
    await panel.trigger('keydown', { key: 'Tab' })
    expect(document.activeElement).toBe(closeButtonElement)
  })
})

function mountDialog(open: boolean): VueWrapper {
  const host = document.createElement('div')
  document.body.append(host)
  const wrapper = mount(AppDialog, {
    attachTo: host,
    props: {
      open,
      title: 'Edit order',
    },
    slots: {
      default: '<input aria-label="Order amount" />',
      actions: '<button type="button" data-testid="save">Save</button>',
    },
  })
  mountedWrappers.push(wrapper)
  return wrapper
}
