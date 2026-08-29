import { flushPromises, mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import CustomersPage from './CustomersPage.vue'

const customerApi = vi.hoisted(() => ({
  getCustomer: vi.fn(),
  postCustomer: vi.fn(),
}))

vi.mock('@/shared/api', async (importOriginal) => ({
  ...(await importOriginal<typeof import('@/shared/api')>()),
  ...customerApi,
}))

const customer = {
  id: '20000000-0000-4000-8000-000000000001',
  displayName: 'Ada Lovelace',
}

function mountCustomersPage() {
  return mount(CustomersPage, {
    global: {
      plugins: [
        createTestingPinia({
          createSpy: vi.fn,
          stubActions: false,
        }),
      ],
    },
  })
}

describe('customers page', () => {
  beforeEach(() => {
    vi.clearAllMocks()
    customerApi.getCustomer.mockResolvedValue(customer)
    customerApi.postCustomer.mockResolvedValue(customer)
  })

  it('creates a customer and renders its identifier', async () => {
    const wrapper = mountCustomersPage()

    await wrapper.get('input[name="displayName"]').setValue('  Ada Lovelace  ')
    await wrapper.get('.customer-form__body').trigger('submit')
    await flushPromises()

    expect(customerApi.postCustomer).toHaveBeenCalledWith({
      displayName: 'Ada Lovelace',
    })
    expect(wrapper.text()).toContain('Customer created')
    expect(wrapper.text()).toContain(customer.id)
  })

  it('validates an identifier before resolving the customer', async () => {
    const wrapper = mountCustomersPage()
    const input = wrapper.get('input[name="lookupCustomerId"]')

    await input.setValue('not-a-guid')
    await wrapper.get('.customer-lookup__form').trigger('submit')

    expect(wrapper.text()).toContain('Enter a valid customer UUID.')
    expect(customerApi.getCustomer).not.toHaveBeenCalled()

    await input.setValue(customer.id)
    await wrapper.get('.customer-lookup__form').trigger('submit')
    await flushPromises()

    expect(customerApi.getCustomer).toHaveBeenCalledWith(customer.id)
    expect(wrapper.text()).toContain('Customer found')
    expect(wrapper.text()).toContain('Ada Lovelace')
  })
})
