import { describe, it, expect, vi, beforeEach } from 'vitest'
import { mount } from '@vue/test-utils'

// jsdom no puede dibujar en un <canvas>, así que reemplazamos Chart.js por un mock
// y verificamos con qué datos se crea el gráfico.
const { ChartMock, destroyMock } = vi.hoisted(() => {
  const destroyMock = vi.fn()
  const ChartMock = vi.fn(() => ({ destroy: destroyMock }))
  ChartMock.register = vi.fn()
  return { ChartMock, destroyMock }
})

vi.mock('chart.js', () => ({
  Chart: ChartMock,
  PieController: {},
  ArcElement: {},
  Tooltip: {},
  Legend: {}
}))

import GraficoCartera from '../src/components/GraficoCartera.vue'

const ITEMS = [
  { cryptoCode: 'BTC', cantidad: 0.03, precioUnitario: 102000000, valorTotal: 3060000 },
  { cryptoCode: 'eth', cantidad: 0.03, precioUnitario: 2900000, valorTotal: 87000 }
]

describe('GraficoCartera', () => {
  beforeEach(() => {
    ChartMock.mockClear()
    destroyMock.mockClear()
  })

  it('crea un gráfico de torta con una porción por criptomoneda', () => {
    mount(GraficoCartera, { props: { items: ITEMS } })

    expect(ChartMock).toHaveBeenCalledTimes(1)
    const config = ChartMock.mock.calls[0][1]
    expect(config.type).toBe('pie')
    expect(config.data.labels).toEqual(['BTC', 'ETH'])
    expect(config.data.datasets[0].data).toEqual([3060000, 87000])
  })

  it('usa un color distinto para cada cripto', () => {
    mount(GraficoCartera, { props: { items: ITEMS } })

    const colores = ChartMock.mock.calls[0][1].data.datasets[0].backgroundColor
    expect(new Set(colores).size).toBe(2)
  })

  it('el tooltip muestra el valor en pesos y el porcentaje', () => {
    mount(GraficoCartera, { props: { items: ITEMS } })

    const label = ChartMock.mock.calls[0][1].options.plugins.tooltip.callbacks.label
    const texto = label({ label: 'ETH', parsed: 87000 })
    expect(texto).toContain('ETH')
    expect(texto).toContain('87.000,00')
    expect(texto).toContain('(2.8%)')
  })

  it('redibuja el gráfico cuando cambian los datos (otro cliente)', async () => {
    const wrapper = mount(GraficoCartera, { props: { items: ITEMS } })

    await wrapper.setProps({ items: [{ cryptoCode: 'USDC', cantidad: 10, valorTotal: 17500 }] })

    expect(destroyMock).toHaveBeenCalledTimes(1)
    expect(ChartMock).toHaveBeenCalledTimes(2)
    expect(ChartMock.mock.calls[1][1].data.labels).toEqual(['USDC'])
  })

  it('destruye el gráfico al salir de la pantalla', () => {
    const wrapper = mount(GraficoCartera, { props: { items: ITEMS } })
    wrapper.unmount()
    expect(destroyMock).toHaveBeenCalled()
  })
})
