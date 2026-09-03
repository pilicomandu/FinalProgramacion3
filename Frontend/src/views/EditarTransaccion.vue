<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const route = useRoute()
const router = useRouter()
const transaction = ref({
  cryptoCode: '',
  action: '',
  clientId: '',
  cryptoAmount: 0,
  money: 0,
  dateTime: ''
})
const loading = ref(true)
const error = ref('')
const guardando = ref(false)

async function obtenerTransaccion() {
  const id = route.params.id
  try {
    const response = await fetch(`https://localhost:7075/api/Transactions/${id}`)
    if (response.ok) {
      const data = await response.json()
      if (data.dateTime) {
        data.dateTime = data.dateTime.replace('Z', '').slice(0, 16)
      }
      transaction.value = data
    } else {
      error.value = 'No se encontró la transacción'
    }
  } catch (err) {
    error.value = 'Error al cargar los datos'
  } finally {
    loading.value = false
  }
}

async function guardarEdicion() {
  guardando.value = true
  const id = route.params.id
  
  const datosActualizados = {
    cryptoCode: transaction.value.cryptoCode,
    action: transaction.value.action,
    clientId: Number(transaction.value.clientId),
    cryptoAmount: Number(transaction.value.cryptoAmount),
    money: Number(transaction.value.money),
    dateTime: transaction.value.dateTime
  }

  try {
    const response = await fetch(`https://localhost:7075/api/Transactions/${id}`, {
      method: 'PATCH',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify(datosActualizados)
    })

    if (response.ok) {
      alert('Transacción actualizada correctamente')
      router.push('/historial')
    } else {
      const errorText = await response.text()
      alert('Error al actualizar: ' + errorText)
    }
  } catch (err) {
    alert('Error de conexión')
  } finally {
    guardando.value = false
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
    <h2>Editar Transacción</h2>
    
    <div v-if="loading">Cargando...</div>
    <div v-else-if="error" style="color: red;">{{ error }}</div>
    <div v-else style="background: #ffffff; padding: 20px; border-radius: 8px;">
      <div>
        <label>Criptomoneda:</label>
        <select v-model="transaction.cryptoCode" style="padding: 5px; margin-left: 10px;">
          <option value="BTC">Bitcoin</option>
          <option value="ETH">Ethereum</option>
          <option value="USDC">USDC</option>
        </select>
      </div>

      <br />

      <div>
        <label>Acción:</label>
        <select v-model="transaction.action" style="padding: 5px; margin-left: 10px;">
          <option value="purchase">Compra</option>
          <option value="sale">Venta</option>
        </select>
      </div>

      <br />

      <div>
        <label>Cliente ID:</label>
        <input type="number" v-model.number="transaction.clientId" style="padding: 5px; margin-left: 10px;" />
      </div>

      <br />

      <div>
        <label>Cantidad:</label>
        <input type="number" step="0.00001" v-model.number="transaction.cryptoAmount" style="padding: 5px; margin-left: 10px;" />
      </div>

      <br />

      <div>
        <label>Monto (ARS):</label>
        <input type="number" step="0.01" v-model.number="transaction.money" style="padding: 5px; margin-left: 10px;" />
      </div>

      <br />

      <div>
        <label>Fecha:</label>
        <input type="datetime-local" v-model="transaction.dateTime" style="padding: 5px; margin-left: 10px;" />
      </div>

      <br />

      <button @click="guardarEdicion" :disabled="guardando" style="padding: 8px 20px; background: #4CAF50; color: white; border: none; border-radius: 4px; cursor: pointer;">
        {{ guardando ? 'Guardando...' : 'Guardar Cambios' }}
      </button>
      <button @click="volver" style="margin-left: 10px; padding: 8px 20px; background: #f44336; color: white; border: none; border-radius: 4px; cursor: pointer;">
        Cancelar
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

label {
  display: inline-block;
  width: 120px;
  font-weight: bold;
}
</style>