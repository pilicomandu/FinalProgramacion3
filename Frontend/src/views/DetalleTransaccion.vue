<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const transaction = ref(null)
const loading = ref(true)
const error = ref('')

async function obtenerTransaccion() {
  const id = route.params.id
  try {
    const response = await fetch(`https://localhost:7075/api/Transactions/${id}`)
    if (response.ok) {
      transaction.value = await response.json()
    } else {
      error.value = 'No se encontró la transacción'
    }
  } catch (err) {
    error.value = 'Error al cargar los datos'
  } finally {
    loading.value = false
  }
}

function volver() {
  router.push('/historial')
}

onMounted(() => {
  obtenerTransaccion()
})
</script>

<template>
  <div class="container">
    <h2>Detalle de Transacción</h2>
    
    <div v-if="loading">Cargando...</div>
    <div v-else-if="error" style="color: red;">{{ error }}</div>
    <div v-else>
      <table border="1" cellpadding="10" style="background: #ffffff;">
        <tr>
          <td><strong>ID:</strong></td>
          <td>{{ transaction.id }}</td>
        </tr>
        <tr>
          <td><strong>Criptomoneda:</strong></td>
          <td>{{ transaction.cryptoCode }}</td>
        </tr>
        <tr>
          <td><strong>Acción:</strong></td>
          <td>
            <span :style="{ color: transaction.action === 'purchase' ? 'green' : 'red' }">
              {{ transaction.action === 'purchase' ? 'Compra' : 'Venta' }}
            </span>
          </td>
        </tr>
        <tr>
          <td><strong>Cantidad:</strong></td>
          <td>{{ transaction.cryptoAmount }}</td>
        </tr>
        <tr>
          <td><strong>Monto (ARS):</strong></td>
          <td>$ {{ transaction.money?.toFixed(2) || transaction.money }}</td>
        </tr>
        <tr>
          <td><strong>Cliente ID:</strong></td>
          <td>{{ transaction.clientId }}</td>
        </tr>
        <tr>
          <td><strong>Fecha:</strong></td>
          <td>{{ new Date(transaction.dateTime).toLocaleString() }}</td>
        </tr>
      </table>

      <br />
      <button @click="volver" style="padding: 8px 20px; background: #2196F3; color: white; border: none; border-radius: 4px; cursor: pointer;">
        Volver al Historial
      </button>
    </div>
  </div>
</template>

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

table {
  background: #ffffff;
  border-radius: 8px;
  width: 100%;
}

td {
  padding: 10px;
}
</style>