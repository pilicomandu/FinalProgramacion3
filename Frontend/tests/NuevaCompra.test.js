import { describe, it, expect, vi, beforeEach, afterEach } from 'vitest'
import { mount, flushPromises } from '@vue/test-utils'
import NuevaCompra from '../src/components/NuevaCompra.vue'

const CLIENTES = [
  { id: 1, name: 'Juan Perez', email: 'juan@mail.com' },
  { id: 2, name: 'Ana Urain', email: 'ana@mail.com' }
]

// Simula una respuesta de fetch: { ok, json(), text() }
function respuesta(datos, ok = true) {
  return {
    ok,
    json: async () => datos,
    text: async () => (typeof datos === 'string' ? datos : JSON.stringify(datos))
  }
}

// Completa el formulario con datos válidos (cada campo se puede pisar)
async function completarFormulario(wrapper, datos = {}) {
  const { crypto = 'BTC', cliente = 1, cantidad = '0.005', fecha = '2026-11-25T10:30' } = datos
  const selects = wrapper.findAll('select')
  await selects[0].setValue(crypto)
  await selects[1].setValue(cliente)
  await wrapper.find('input[type="number"]').setValue(cantidad)
  await wrapper.find('input[type="datetime-local"]').setValue(fecha)
}

describe('NuevaCompra', () => {
  beforeEach(() => {
    // No hay backend en los tests: reemplazamos fetch y alert por funciones simuladas (mocks)
    globalThis.fetch = vi.fn().mockResolvedValueOnce(respuesta(CLIENTES))
    globalThis.alert = vi.fn()
  })

  afterEach(() => {
    vi.restoreAllMocks()
  })

  it('carga los clientes en el select al montarse', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()

    expect(fetch).toHaveBeenCalledWith('https://localhost:7075/api/Clients')
    const opciones = wrapper.findAll('select')[1].findAll('option')
    // 1 opción vacía + 2 clientes
    expect(opciones).toHaveLength(3)
    expect(opciones[1].text()).toBe('Juan Perez')
    expect(opciones[2].text()).toBe('Ana Urain')
  })

  it('ofrece al menos 3 criptomonedas', async () => {
    const wrapper = mount(NuevaCompra)
    const valores = wrapper.findAll('select')[0].findAll('option').map(o => o.element.value)
    expect(valores).toEqual(['', 'BTC', 'ETH', 'USDC'])
  })

  it('no envía la compra si no se eligió criptomoneda', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()
    await completarFormulario(wrapper, { crypto: '' })

    await wrapper.find('button').trigger('click')

    expect(alert).toHaveBeenCalledWith('Debe seleccionar una criptomoneda')
    expect(fetch).toHaveBeenCalledTimes(1) // solo la carga de clientes
  })

  it('no envía la compra si la cantidad es 0 o negativa', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()

    for (const cantidad of ['0', '-2']) {
      await completarFormulario(wrapper, { cantidad })
      await wrapper.find('button').trigger('click')
      expect(alert).toHaveBeenLastCalledWith('La cantidad debe ser mayor a cero')
    }
    expect(fetch).toHaveBeenCalledTimes(1)
  })

  it('no envía la compra si no se eligió cliente', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()
    await completarFormulario(wrapper, { cliente: '' })

    await wrapper.find('button').trigger('click')

    expect(alert).toHaveBeenCalledWith('Debe seleccionar un cliente')
    expect(fetch).toHaveBeenCalledTimes(1)
  })

  it('no envía la compra si no se ingresó la fecha', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()
    await completarFormulario(wrapper, { fecha: '' })

    await wrapper.find('button').trigger('click')

    expect(alert).toHaveBeenCalledWith('Debe ingresar la fecha y hora')
    expect(fetch).toHaveBeenCalledTimes(1)
  })

  it('envía un POST con los datos correctos y limpia el formulario', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()
    fetch.mockResolvedValueOnce(respuesta({ id: 10 }))
    await completarFormulario(wrapper)

    await wrapper.find('button').trigger('click')
    await flushPromises()

    const [url, opciones] = fetch.mock.calls[1]
    expect(url).toBe('https://localhost:7075/api/Transactions')
    expect(opciones.method).toBe('POST')
    expect(JSON.parse(opciones.body)).toEqual({
      cryptoCode: 'BTC',
      action: 'purchase',
      clientId: 1,
      cryptoAmount: 0.005,
      dateTime: '2026-11-25T10:30'
    })
    expect(alert).toHaveBeenCalledWith('Compra guardada correctamente')
    expect(wrapper.findAll('select')[0].element.value).toBe('')
  })

  it('muestra el error que devuelve el backend', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()
    fetch.mockResolvedValueOnce(respuesta('El cliente seleccionado no existe.', false))
    await completarFormulario(wrapper)

    await wrapper.find('button').trigger('click')
    await flushPromises()

    expect(alert).toHaveBeenCalledWith('Error al guardar la compra:\nEl cliente seleccionado no existe.')
  })

  it('avisa si no se puede conectar con el servidor', async () => {
    const wrapper = mount(NuevaCompra)
    await flushPromises()
    fetch.mockRejectedValueOnce(new Error('Failed to fetch'))
    vi.spyOn(console, 'error').mockImplementation(() => {})
    await completarFormulario(wrapper)

    await wrapper.find('button').trigger('click')
    await flushPromises()

    expect(alert).toHaveBeenCalledWith('No se pudo conectar con el servidor.')
  })
})
