<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const transactions = ref([])
const clients = ref([])
const selectedClient = ref('')
const loading = ref(false)

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

async function obtenerTransacciones() {
  if (!selectedClient.value) {
    alert('Por favor, seleccione un cliente')
    return
  }

  loading.value = true
  try {
    const response = await fetch(`https://localhost:7075/api/Transactions/client/${selectedClient.value}`)
    if (response.ok) {
      const data = await response.json()
      transactions.value = data
    } else {
      alert('Error al obtener las transacciones')
      transactions.value = []
    }
  } catch (error) {
    console.error('Error:', error)
    alert('Error de conexión con el servidor')
    transactions.value = []
  } finally {
    loading.value = false
  }
}

async function eliminarTransaccion(id) {
  if (!confirm('¿Está seguro que desea eliminar esta transacción?')) {
    return
  }

  try {
    const response = await fetch(`https://localhost:7075/api/Transactions/${id}`, {
      method: 'DELETE'
    })
    
    if (response.ok) {
      alert('Transacción eliminada correctamente')
      await obtenerTransacciones()
    } else {
      const error = await response.text()
      alert('Error al eliminar: ' + error)
    }
  } catch (error) {
    console.error('Error:', error)
    alert('Error de conexión con el servidor')
  }
}

function verDetalle(id) {
  router.push(`/transaccion/${id}`)
}

function editarTransaccion(id) {
  router.push(`/transaccion/${id}/editar`)
}

function volver() {
  router.push('/')
}

onMounted(() => {
  obtenerClientes()
})
</script>

<template>
  <div class="container">
    <h2>Historial de Movimientos</h2>

    <div style="margin-bottom: 20px;">
      <label style="margin-right: 10px; font-weight: bold;">Cliente:</label>
      <select v-model="selectedClient" style="padding: 5px; width: 200px;">
        <option value="">Seleccione un cliente</option>
        <option v-for="client in clients" :key="client.id" :value="client.id">
          {{ client.name }}
        </option>
      </select>
      
      <button 
        @click="obtenerTransacciones" 
        :disabled="loading" 
        style="margin-left: 10px; padding: 5px 15px; cursor: pointer; background: #4CAF50; color: white; border: none; border-radius: 4px;"
      >
        {{ loading ? 'Cargando...' : 'Ver movimientos' }}
      </button>
      
      <button 
        @click="volver" 
        style="margin-left: 10px; padding: 5px 15px; cursor: pointer; background: #f44336; color: white; border: none; border-radius: 4px;"
      >
        Volver
      </button>
    </div>

    <div v-if="loading" style="text-align: center; padding: 20px;">
      Cargando transacciones...
    </div>

    <div v-else-if="transactions.length > 0">
      <p style="color: #666;">Mostrando {{ transactions.length }} transacción(es)</p>
      
      <table border="1" cellpadding="8" cellspacing="0" style="width: 100%; border-collapse: collapse;">
        <thead style="background: #f0f0f0;">
          <tr>
            <th>ID</th>
            <th>Cripto</th>
            <th>Acción</th>
            <th>Cantidad</th>
            <th>Monto (ARS)</th>
            <th>Fecha</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="transaction in transactions" :key="transaction.id" style="border-bottom: 1px solid #ddd;">
            <td style="text-align: center;">{{ transaction.id }}</td>
            <td style="text-align: center;">
              <strong>{{ transaction.cryptoCode }}</strong>
            </td>
            <td style="text-align: center;">
              <span 
                :style="{ 
                  color: transaction.action === 'purchase' ? 'green' : 'red',
                  fontWeight: 'bold'
                }"
              >
                {{ transaction.action === 'purchase' ? 'Compra' : 'Venta' }}
              </span>
            </td>
            <td style="text-align: right;">{{ transaction.cryptoAmount.toFixed(8) }}</td>
            <td style="text-align: right;">
              $ {{ transaction.money?.toFixed(2) || transaction.money }}
            </td>
            <td style="text-align: center;">
              {{ new Date(transaction.dateTime).toLocaleString() }}
            </td>
            <td style="text-align: center; white-space: nowrap;">
              <button 
                @click="verDetalle(transaction.id)" 
                style="margin: 2px; padding: 4px 10px; cursor: pointer; background: #2196F3; color: white; border: none; border-radius: 4px;"
              >
                Ver
              </button>
              <button 
                @click="editarTransaccion(transaction.id)" 
                style="margin: 2px; padding: 4px 10px; cursor: pointer; background: #FF9800; color: white; border: none; border-radius: 4px;"
              >
                Editar
              </button>
              <button 
                @click="eliminarTransaccion(transaction.id)" 
                style="margin: 2px; padding: 4px 10px; cursor: pointer; background: #f44336; color: white; border: none; border-radius: 4px;"
              >
                Eliminar
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-else style="text-align: center; padding: 40px; color: #666; background: #ffffff; border-radius: 8px;">
      <p style="font-size: 18px;">No hay transacciones para este cliente</p>
      <p style="font-size: 14px;">Seleccione otro cliente o cree una nueva compra/venta</p>
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
  text-align: center;
}

td {
  padding: 8px 12px;
}

tbody tr:nth-child(even) {
  background-color: #f9f9f9;
}

tbody tr:hover {
  background-color: #f0f0f0;
}

select, label {
  font-size: 14px;
}
</style>