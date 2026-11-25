<script setup>
// Gráfico de torta que muestra la composición de la cartera:
// qué porcentaje del dinero total representa cada criptomoneda.
import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { Chart, PieController, ArcElement, Tooltip, Legend } from 'chart.js'

// Chart.js trabaja por módulos: registramos solo lo que usa un gráfico de torta
Chart.register(PieController, ArcElement, Tooltip, Legend)

// Recibe la lista que devuelve el backend: [{ cryptoCode, cantidad, valorTotal, ... }]
const props = defineProps({
  items: { type: Array, required: true }
})

const COLORES = {
  BTC: '#F7931A',
  ETH: '#627EEA',
  USDC: '#2775CA'
}

const canvas = ref(null)
let grafico = null

function formatearPesos(valor) {
  return '$ ' + Number(valor).toLocaleString('es-AR', { minimumFractionDigits: 2, maximumFractionDigits: 2 })
}

function dibujar() {
  // Si ya había un gráfico (por ejemplo, de otro cliente), lo destruimos antes de crear uno nuevo
  if (grafico) grafico.destroy()

  const total = props.items.reduce((suma, item) => suma + Number(item.valorTotal), 0)

  grafico = new Chart(canvas.value, {
    type: 'pie',
    data: {
      labels: props.items.map(item => item.cryptoCode.toUpperCase()),
      datasets: [{
        data: props.items.map(item => Number(item.valorTotal)),
        backgroundColor: props.items.map(item => COLORES[item.cryptoCode.toUpperCase()] || '#999999'),
        borderColor: '#ffffff',
        borderWidth: 2
      }]
    },
    options: {
      responsive: true,
      maintainAspectRatio: false,
      plugins: {
        legend: { position: 'right' },
        tooltip: {
          callbacks: {
            // Al pasar el mouse: "BTC: $ 102.472,17 (84,9%)"
            label: (contexto) => {
              const porcentaje = total > 0 ? (contexto.parsed / total * 100).toFixed(1) : 0
              return `${contexto.label}: ${formatearPesos(contexto.parsed)} (${porcentaje}%)`
            }
          }
        }
      }
    }
  })
}

onMounted(dibujar)

// Si cambian los datos (se analiza otro cliente), se vuelve a dibujar
watch(() => props.items, dibujar, { deep: true })

// Al salir de la pantalla liberamos el gráfico
onBeforeUnmount(() => {
  if (grafico) grafico.destroy()
})
</script>

<template>
  <div class="grafico">
    <h3>Composición de la cartera</h3>
    <div class="contenedor">
      <canvas ref="canvas"></canvas>
    </div>
  </div>
</template>

<style scoped>
.grafico {
  background: #ffffff;
  padding: 15px;
  border-radius: 8px;
  margin-top: 20px;
  border: 1px solid #ddd;
}

.grafico h3 {
  margin-top: 0;
}

.contenedor {
  position: relative;
  height: 300px;
  max-width: 500px;
  margin: 0 auto;
}
</style>
