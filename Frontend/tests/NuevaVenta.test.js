import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import NuevaVenta from '../src/components/NuevaVenta.vue'

function respuesta(datos, ok = true) {
  return {
    ok,
    json: async () => datos,
    text: async () => (typeof datos === 'string' ? datos : JSON.stringify(datos))
  }
}

async function completarFormulario(wrapper) {
  const selects = wrapper.findAll('select')
  await selects[0].setValue('ETH')
  await selects[1].setValue(1)
  await wrapper.find('input[type="number"]').setValue('0.5')
  await wrapper.find('input[type="datetime-local"]').setValue('2026-11-25T18:00')
}

describe('NuevaVenta', () => {
  beforeEach(() => {
    globalThis.fetch = vi.fn().mockResolvedValueOnce(respuesta([{ id: 1, name: 'Juan Perez' }]))
    globalThis.alert = vi.fn()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('envía la transacción con action "sale"', async () => {
    const wrapper = mount(NuevaVenta)
    await flushPromises()
    fetch.mockResolvedValueOnce(respuesta({ id: 11 }))
    await completarFormulario(wrapper)

    await wrapper.find('button').trigger('click')
    await flushPromises()

    const cuerpo = JSON.parse(fetch.mock.calls[1][1].body)
    expect(cuerpo.action).toBe('sale')
    expect(cuerpo.cryptoCode).toBe('ETH')
    expect(cuerpo.cryptoAmount).toBe(0.5)
    expect(alert).toHaveBeenCalledWith('Venta guardada correctamente')
  })

  it('muestra el error cuando el cliente no tiene saldo suficiente', async () => {
    const wrapper = mount(NuevaVenta)
    await flushPromises()
    const mensaje = 'No se puede vender 0.5 ETH. Saldo disponible: 0 ETH'
    fetch.mockResolvedValueOnce(respuesta(mensaje, false))
    await completarFormulario(wrapper)

    await wrapper.find('button').trigger('click')
    await flushPromises()

    expect(alert).toHaveBeenCalledWith('Error al guardar la venta:\n' + mensaje)
  })

  it('no envía la venta si la cantidad no es mayor a 0', async () => {
    const wrapper = mount(NuevaVenta)
    await flushPromises()
    await completarFormulario(wrapper)
    await wrapper.find('input[type="number"]').setValue('0')

    await wrapper.find('button').trigger('click')

    expect(alert).toHaveBeenCalledWith('La cantidad debe ser mayor a cero')
    expect(fetch).toHaveBeenCalledTimes(1)
  })
})
