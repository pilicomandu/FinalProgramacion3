<template>
  <div class="container">
    <h1>Billetera de Criptomonedas</h1>
  
    <div class="top-buttons">
      <button @click="mostrarCliente = !mostrarCliente" class="btn-cliente">
        {{ mostrarCliente ? 'Ocultar' : 'Agregar Nuevo Cliente' }}
      </button>
      <button @click="$router.push('/clientes')" class="btn-ver-clientes">
        Ver Clientes
      </button>
    </div>

    <div v-if="mostrarCliente" class="client-form">
      <h3>Nuevo Cliente</h3>
      <div>
        <label>Nombre:</label>
        <input type="text" v-model="nuevoCliente.nombre" placeholder="Ej: Juan Pérez" />
      </div>
      <div>
        <label>Email:</label>
        <input type="email" v-model="nuevoCliente.email" placeholder="ejemplo@mail.com" />
      </div>
      <button @click="crearCliente" :disabled="cargandoCliente">
        {{ cargandoCliente ? 'Guardando...' : 'Guardar Cliente' }}
      </button>
      <button @click="mostrarCliente = false" style="margin-left: 10px; background: #999;">
        Cancelar
      </button>
    </div>

    <hr />

    <div class="nav-tabs">
      <button 
        @click="vistaActual = 'compra'" 
        :class="{ active: vistaActual === 'compra' }"
      >
        Nueva Compra
      </button>
      <button 
        @click="vistaActual = 'venta'" 
        :class="{ active: vistaActual === 'venta' }"
      >
        Nueva Venta
      </button>
      <button @click="$router.push('/historial')">
        Historial
      </button>
      <button @click="$router.push('/analisis')">
        Analisis
      </button>
    </div>

    <div class="form-container">
      <NuevaCompra v-if="vistaActual === 'compra'" />
      <NuevaVenta v-if="vistaActual === 'venta'" />
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import NuevaCompra from '../components/NuevaCompra.vue'
import NuevaVenta from '../components/NuevaVenta.vue'

const vistaActual = ref('compra')
const mostrarCliente = ref(false)
const cargandoCliente = ref(false)

const nuevoCliente = ref({
  nombre: '',
  email: ''
})

async function crearCliente() {
  if (!nuevoCliente.value.nombre.trim()) {
    alert('El nombre es obligatorio')
    return
  }
  if (!nuevoCliente.value.email.trim()) {
    alert('El email es obligatorio')
    return
  }
  if (!nuevoCliente.value.email.includes('@') || !nuevoCliente.value.email.includes('.')) {
    alert('El email no tiene un formato válido')
    return
  }

  cargandoCliente.value = true
  try {
    const response = await fetch('https://localhost:7075/api/Clients', {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json'
      },
      body: JSON.stringify({
        name: nuevoCliente.value.nombre,
        email: nuevoCliente.value.email
      })
    })

    if (response.ok) {
      alert('Cliente creado correctamente')
      nuevoCliente.value.nombre = ''
      nuevoCliente.value.email = ''
      mostrarCliente.value = false
      window.location.reload()
    } else {
      const error = await response.text()
      alert('Error al crear cliente: ' + error)
    }
  } catch (error) {
    console.error('Error:', error)
    alert('Error de conexión con el servidor')
  } finally {
    cargandoCliente.value = false
  }
}
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

h1 {
  color: #333;
  text-align: center;
  margin-bottom: 20px;
}

/* Botones centrados arriba */
.top-buttons {
  display: flex;
  justify-content: center;
  gap: 15px;
  margin-bottom: 20px;
  flex-wrap: wrap;
}

.btn-cliente {
  padding: 10px 24px;
  background: #2196F3;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.3s;
}

.btn-cliente:hover {
  background: #1976D2;
}

.btn-ver-clientes {
  padding: 10px 24px;
  background: #607D8B;
  color: white;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.3s;
}

.btn-ver-clientes:hover {
  background: #455A64;
}

/* Formulario de cliente */
.client-form {
  margin-top: 15px;
  padding: 20px;
  border: 2px solid #2196F3;
  border-radius: 8px;
  background: #ffffff;
}

.client-form h3 {
  margin-top: 0;
  color: #333;
}

.client-form div {
  margin-bottom: 12px;
}

.client-form label {
  display: inline-block;
  width: 80px;
  font-weight: bold;
  color: #555;
}

.client-form input {
  padding: 8px 12px;
  border: 1px solid #ccc;
  border-radius: 4px;
  width: 300px;
  font-size: 14px;
}

.client-form input:focus {
  outline: none;
  border-color: #2196F3;
  box-shadow: 0 0 5px rgba(33, 150, 243, 0.3);
}

.client-form button {
  padding: 8px 20px;
  background: #4CAF50;
  color: white;
  border: none;
  border-radius: 4px;
  cursor: pointer;
  font-size: 14px;
  transition: background 0.3s;
}

.client-form button:hover {
  background: #45a049;
}

.client-form button:disabled {
  background: #ccc;
  cursor: not-allowed;
}

hr {
  margin: 25px 0;
  border: none;
  border-top: 2px solid #d0d0d0;
}

.nav-tabs {
  display: flex;
  gap: 10px;
  margin-bottom: 25px;
  flex-wrap: wrap;
  justify-content: center;
}

.nav-tabs button {
  padding: 12px 24px;
  background: #ffffff;
  border: 2px solid #ccc;
  border-radius: 8px;
  cursor: pointer;
  font-size: 15px;
  font-weight: 500;
  transition: all 0.3s;
  color: #333;
}

.nav-tabs button:hover {
  background: #e0e0e0;
  transform: translateY(-2px);
  box-shadow: 0 2px 8px rgba(0,0,0,0.1);
}

.nav-tabs button.active {
  background: #4CAF50;
  color: white;
  border-color: #4CAF50;
}

.nav-tabs button.active:hover {
  background: #45a049;
}

.form-container {
  display: flex;
  justify-content: center;
}

.form-container > * {
  width: 100%;
  max-width: 500px;
}

@media (max-width: 600px) {
  .client-form input {
    width: 100%;
  }
  
  .nav-tabs button {
    flex: 1;
    min-width: 120px;
    text-align: center;
  }

  .form-container > * {
    max-width: 100%;
  }
}
</style>