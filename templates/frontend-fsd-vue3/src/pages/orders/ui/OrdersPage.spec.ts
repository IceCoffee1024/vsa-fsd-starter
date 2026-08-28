import { flushPromises, mount } from '@vue/test-utils'
import { createTestingPinia } from '@pinia/testing'
import { beforeEach, describe, expect, it, vi } from 'vitest'
import OrdersPage from './OrdersPage.vue'

const orderApi = vi.hoisted(() => ({
  deleteOrder: vi.fn(),
  getOrder: vi.fn(),
  getOrders: vi.fn(),
  postOrder: vi.fn(),
  postOrdersBatch: vi.fn(),
  postOrdersBatchDelete: vi.fn(),
  putOrder: vi.fn(),
}))

vi.mock('@/entities/order/api/orderApi', () => orderApi)

const customerId = '20000000-0000-4000-8000-000000000001'
const orderId = '10000000-0000-4000-8000-000000000001'

function mountOrdersPage() {
  return mount(OrdersPage, {
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

describe('orders page', () => {
  beforeEach(() => {
    orderApi.deleteOrder.mockResolvedValue(undefined)
    orderApi.getOrder.mockResolvedValue({
      id: orderId,
      customerId,
      customerName: 'Ada Lovelace',
      totalAmount: 42.5,
    })
    orderApi.postOrdersBatch.mockResolvedValue({ createdCount: 0, items: [] })
    orderApi.postOrdersBatchDelete.mockResolvedValue({
      requestedCount: 0,
      deletedCount: 0,
      missingIds: [],
    })
    orderApi.putOrder.mockResolvedValue({
      id: orderId,
      customerId,
      customerName: 'Ada Lovelace',
      totalAmount: 42.5,
    })
    orderApi.getOrders.mockResolvedValue([
      {
        id: orderId,
        customerId,
        customerName: 'Ada Lovelace',
        totalAmount: 42.5,
      },
    ])
  })

  it('loads orders and adds a created order to the visible table', async () => {
    orderApi.postOrder.mockResolvedValue({
      id: '10000000-0000-4000-8000-000000000002',
      customerId,
      customerName: 'Ada Lovelace',
      totalAmount: 88.25,
    })
    const wrapper = mountOrdersPage()
    await flushPromises()

    expect(wrapper.text()).toContain('Ada Lovelace')
    expect(wrapper.text()).toContain('42.50')

    await wrapper.find('input[name="customerId"]').setValue(customerId)
    await wrapper.find('input[name="totalAmount"]').setValue('88.25')
    await wrapper.find('form').trigger('submit')
    await flushPromises()

    expect(orderApi.postOrder).toHaveBeenCalledWith({
      customerId,
      totalAmount: 88.25,
    })
    expect(wrapper.text()).toContain('88.25')
    expect(wrapper.text()).toContain('2 records')
    expect(wrapper.text()).toContain('Order 10000000 created.')
  })

  it('edits and deletes an order through the row actions', async () => {
    orderApi.putOrder.mockResolvedValue({
      id: orderId,
      customerId,
      customerName: 'Ada Lovelace',
      totalAmount: 64,
    })
    const wrapper = mountOrdersPage()
    await flushPromises()

    await wrapper
      .get(`button[aria-label="Edit order ${orderId}"]`)
      .trigger('click')
    await wrapper.get('input[name="editTotalAmount"]').setValue('64')
    await wrapper.get('#edit-order-form').trigger('submit')
    await flushPromises()

    expect(orderApi.putOrder).toHaveBeenCalledWith(orderId, { totalAmount: 64 })
    expect(wrapper.text()).toContain('64.00')

    await wrapper
      .get(`button[aria-label="Delete order ${orderId}"]`)
      .trigger('click')
    await wrapper.get('.dialog-panel .button--danger').trigger('click')
    await flushPromises()

    expect(orderApi.deleteOrder).toHaveBeenCalledWith(orderId)
    expect(wrapper.text()).toContain('No orders yet')
  })

  it('creates and deletes orders in batches', async () => {
    const batchOrder = {
      id: '10000000-0000-4000-8000-000000000002',
      customerId,
      customerName: 'Ada Lovelace',
      totalAmount: 75,
    }
    orderApi.postOrdersBatch.mockResolvedValue({
      createdCount: 1,
      items: [batchOrder],
    })
    orderApi.postOrdersBatchDelete.mockResolvedValue({
      requestedCount: 1,
      deletedCount: 1,
      missingIds: [],
    })
    const wrapper = mountOrdersPage()
    await flushPromises()

    const batchCreateButton = wrapper
      .findAll('button')
      .find((button) => button.text().includes('Batch create'))
    await batchCreateButton!.trigger('click')
    await wrapper.get('input[name="orders[0].customerId"]').setValue(customerId)
    await wrapper.get('input[name="orders[0].totalAmount"]').setValue('75')
    await wrapper.get('#batch-create-form').trigger('submit')
    await flushPromises()

    expect(orderApi.postOrdersBatch).toHaveBeenCalledWith({
      orders: [{ customerId, totalAmount: 75 }],
    })

    await wrapper
      .get(`input[aria-label="Select order ${batchOrder.id}"]`)
      .setValue(true)
    const batchDeleteButton = wrapper
      .findAll('button')
      .find((button) => button.text().includes('Delete selected'))
    await batchDeleteButton!.trigger('click')
    await wrapper.get('.dialog-panel .button--danger').trigger('click')
    await flushPromises()

    expect(orderApi.postOrdersBatchDelete).toHaveBeenCalledWith([batchOrder.id])
    expect(wrapper.text()).toContain('1 of 1 deleted')
  })
})
