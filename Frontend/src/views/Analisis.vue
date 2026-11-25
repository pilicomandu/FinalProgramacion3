<template>
  <div class="container">
    <h2>Analisis de Cartera</h2>

    <div style="margin-bottom: 20px;">
      <label style="margin-right: 10px; font-weight: bold;">Cliente:</label>
      <select v-model="selectedClient" style="padding: 5px; width: 250px;">
        <option value="">Seleccione un cliente</option>
        <option v-for="client in clients" :key="client.id" :value="client.id">
          {{ client.name }}
        </option>
      </select>
      
      <button 
        @click="obtenerEstadoCartera" 
        :disabled="loading" 
        style="margin-left: 10px; padding: 5px 20px; cursor: pointer; background: #4CAF50; color: white; border: none; border-radius: 4px; font-size: 14px;"
      >
        {{ loading ? 'Analizando...' : 'Analizar Cartera' }}
      </button>
      
      <button 
        @click="volver" 
        style="margin-left: 10px; padding: 5px 20px; cursor: pointer; background: #f44336; color: white; border: none; border-radius: 4px; font-size: 14px;"
      >
        Volver
      </button>
    </div>

    <div v-if="loading" style="text-align: center; padding: 40px;">
      <p style="font-size: 18px;">Analizando cartera...</p>
      <p style="color: #666;">Consultando precios actuales</p>
    </div>

    <div v-else-if="error" style="text-align: center; padding: 40px; color: #666; background: #fff3cd; border-radius: 8px; border: 1px solid #ffc107;">
      <p style="font-size: 18px;">{{ error }}</p>
    </div>

    <div v-else-if="cartera && cartera.crypto && cartera.crypto.length > 0">
      <div style="background: #ffffff; padding: 15px; border-radius: 8px; margin-bottom: 20px; border: 1px solid #4CAF50;">
        <h3>Cliente: {{ cartera.clientName }}</h3>
        <p style="margin: 5px 0; color: #666;">Email: {{ cartera.clientEmail }}</p>
        <p style="margin: 5px 0; font-size: 18px; font-weight: bold; color: #4CAF50;">
          Total de la cartera: $ {{ formatearPrecio(cartera.total) }}
        </p>
      </div>

      <table border="1" cellpadding="10" cellspacing="0" style="width: 100%; border-collapse: collapse;">
        <thead style="background: #f0f0f0;">
          <tr>
            <th style="text-align: left;">Criptomoneda</th>
            <th style="text-align: right;">Cantidad</th>
            <th style="text-align: right;">Precio Unitario (ARS)</th>
            <th style="text-align: right;">Valor Total (ARS)</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="item in cartera.crypto" :key="item.cryptoCode" style="border-bottom: 1px solid #ddd;">
            <td style="font-weight: bold;">
              {{ item.cryptoCode }}
            </td>
            <td style="text-align: right;">
              {{ formatearNumero(item.cantidad) }}
            </td>
            <td style="text-align: right;">
              $ {{ formatearPrecio(item.precioUnitario) }}
            </td>
            <td style="text-align: right; font-weight: bold; color: #4CAF50;">
              $ {{ formatearPrecio(item.valorTotal) }}
            </td>
          </tr>
        </tbody>
        <tfoot style="background: #f9f9f9; font-weight: bold;">
          <tr>
            <td colspan="3" style="text-align: right; font-size: 16px;">TOTAL</td>
            <td style="text-align: right; font-size: 16px; color: #4CAF50;">
              $ {{ formatearPrecio(cartera.total) }}
            </td>
          </tr>
        </tfoot>
      </table>

      <GraficoCartera :items="cartera.crypto" />
    </div>

    <div v-else-if="cartera && cartera.crypto && cartera.crypto.length === 0" style="text-align: center; padding: 40px; color: #666; background: #ffffff; border-radius: 8px;">
      <p style="font-size: 18px;">Este cliente no tiene criptomonedas en su cartera</p>
      <p style="font-size: 14px;">Realice una compra para comenzar a invertir</p>
    </div>

    <div v-else style="text-align: center; padding: 40px; color: #666; background: #ffffff; border-radius: 8px;">
      <p style="font-size: 18px;">Seleccione un cliente y haga clic en "Analizar Cartera"</p>
      <p style="font-size: 14px;">Podra ver todas sus criptomonedas y su valor en ARS</p>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import GraficoCartera from '../components/GraficoCartera.vue'

const router = useRouter()
const clients = ref([])
const selectedClient = ref('')
const loading = ref(false)
const cartera = ref(null)
const error = ref('')

async function obtenerClientes() {
  try {
    const response = await fetch('https://localhost:7075/api/Clients')
    if (response.ok) {
      const data = await response.json()
      clients.value = data
    } else {
      alert('Error al cargar los clientes')
    }
  } catch (error) {
    console.error('Error:', error)
    alert('Error de conexión con el servidor')
  }
}

async function obtenerEstadoCartera() {
  if (!selectedClient.value) {
    alert('Por favor, seleccione un cliente')
    return
  }

  loading.value = true
  error.value = ''
  cartera.value = null

  try {
    const response = await fetch(`https://localhost:7075/api/Transactions/estado/${selectedClient.value}`)
    
    if (response.ok) {
      const data = await response.json()
      cartera.value = data
      
      if (data.crypto && data.crypto.length === 0) {
        error.value = data.message || 'Este cliente no tiene criptomonedas en su cartera'
      }
    } else {
      const errorText = await response.text()
      error.value = 'Error al obtener el estado: ' + errorText
    }
  } catch (err) {
    console.error('Error:', err)
    error.value = 'Error de conexión con el servidor'
  } finally {
    loading.value = false
  }
}

function formatearNumero(numero) {
  if (numero === undefined || numero === null) return '0,00'
  return numero.toLocaleString('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 8
  })
}

function formatearPrecio(precio) {
  if (precio === undefined || precio === null) return '0,00'
  return precio.toLocaleString('es-AR', {
    minimumFractionDigits: 2,
    maximumFractionDigits: 2
  })
}

function volver() {
  router.push('/')
}

onMounted(() => {
  obtenerClientes()
})
</script>

<style scoped>
.container {
  max-width: 900px;
  margin: 0 auto;
  padding: 20px;
  font-family: Arial, sans-serif;
  background-color: #e8f0f8;
  min-height: 100vh;
  border-radius: 10px;
}

button {
  font-size: 14px;
}

button:hover {
  opacity: 0.8;
}

button:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

table {
  margin-top: 10px;
}

th {
  text-align: left;
  padding: 10px;
}

td {
  padding: 10px;
}

tbody tr:nth-child(even) {
  background-color: #f9f9f9;
}

tbody tr:hover {
  background-color: #f0f0f0;
}

select {
  font-size: 14px;
  padding: 5px;
  border: 1px solid #ddd;
  border-radius: 4px;
}
</style>