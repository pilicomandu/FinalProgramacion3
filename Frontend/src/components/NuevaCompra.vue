<script setup>
import { ref, onMounted } from 'vue'

const cryptoCode = ref('')
const cryptoAmount = ref(0)
const dateTime = ref('')
const clients = ref([])
const clientId = ref('')

async function obtenerClientes() {
  const response = await fetch('https://localhost:7075/api/Clients')
  const data = await response.json()
  clients.value = data
}

onMounted(() => {
  obtenerClientes()
})

async function guardarCompra() {
  if (!cryptoCode.value) { 
    alert("Debe seleccionar una criptomoneda") 
    return 
  }
  if (cryptoAmount.value <= 0) { 
    alert("La cantidad debe ser mayor a cero") 
    return 
  }
  if (!clientId.value) { 
    alert("Debe seleccionar un cliente") 
    return 
  }
  if (!dateTime.value) {
    alert("Debe ingresar la fecha y hora")
    return
  }

  const transaction = { 
    cryptoCode: cryptoCode.value, 
    action: "purchase", 
    clientId: Number(clientId.value), 
    cryptoAmount: Number(cryptoAmount.value), 
    dateTime: dateTime.value 
  }

  try { 
    const response = await fetch("https://localhost:7075/api/Transactions", { 
      method: "POST", 
      headers: { "Content-Type": "application/json" }, 
      body: JSON.stringify(transaction) 
    })
    
    if (response.ok) { 
      cryptoCode.value = '' 
      cryptoAmount.value = 0 
      dateTime.value = '' 
      clientId.value = '' 
      alert("Compra guardada correctamente") 
    } else { 
      const error = await response.text() 
      console.log("Error del backend:", error)
      alert("Error al guardar la compra:\n" + error) 
    } 
  }
  catch (error) { 
    console.error("Error de conexión:", error) 
    alert("No se pudo conectar con el servidor.") 
  }
}
</script>

<template>
  <div class="form-compra">
    <h2>Nueva Compra</h2>

    <div class="campo">
      <label>Criptomoneda:</label>
      <select v-model="cryptoCode">
        <option value="">Seleccione una criptomoneda</option>
        <option value="BTC">Bitcoin</option>
        <option value="ETH">Ethereum</option>
        <option value="USDC">USDC</option>
      </select>
    </div>

    <div class="campo">
      <label>Cliente:</label>
      <select v-model="clientId">
        <option value="">Seleccione un cliente</option>
        <option v-for="client in clients" :key="client.id" :value="client.id">
          {{ client.name }}
        </option>
      </select>
    </div>

    <div class="campo">
      <label>Cantidad:</label>
      <input type="number" step="0.00001" v-model="cryptoAmount" />
    </div>

    <div class="campo">
      <label>Fecha:</label>
      <input type="datetime-local" v-model="dateTime" />
    </div>

    <button @click="guardarCompra" class="btn-guardar">Guardar Compra</button>
  </div>
</template>

<style scoped>
.form-compra {
  background: #ffffff;
  padding: 25px 30px;
  border-radius: 10px;
  box-shadow: 0 2px 10px rgba(0,0,0,0.08);
  width: 100%;
  max-width: 500px;
  margin: 0 auto;
}

.form-compra h2 {
  text-align: center;
  margin-top: 0;
  color: #333;
}

.campo {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 15px;
}

.campo label {
  font-weight: bold;
  color: #555;
  min-width: 120px;
}

.campo select,
.campo input {
  padding: 8px 12px;
  border: 1px solid #ccc;
  border-radius: 4px;
  font-size: 14px;
  flex: 1;
  max-width: 280px;
}

.campo select:focus,
.campo input:focus {
  outline: none;
  border-color: #4CAF50;
  box-shadow: 0 0 5px rgba(76, 175, 80, 0.3);
}

.btn-guardar {
  display: block;
  width: 100%;
  padding: 10px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  font-size: 16px;
  cursor: pointer;
  transition: background 0.3s;
  margin-top: 5px;
}

.btn-guardar:hover {
  background: #45a049;
}

@media (max-width: 600px) {
  .form-compra {
    padding: 15px;
  }

  .campo {
    flex-direction: column;
    align-items: stretch;
  }

  .campo label {
    margin-bottom: 4px;
    min-width: auto;
  }

  .campo select,
  .campo input {
    max-width: 100%;
  }
}
</style>