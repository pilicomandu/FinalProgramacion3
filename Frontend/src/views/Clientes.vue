<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const clients = ref([])
const loading = ref(false)
const mostrarFormulario = ref(false)
const editando = ref(false)

const formCliente = ref({
  id: null,
  name: '',
  email: ''
})

async function obtenerClientes() {
  loading.value = true
  try {
    const response = await fetch('https://localhost:7075/api/Clients')
    if (response.ok) {
      clients.value = await response.json()
    } else {
      alert('Error al cargar los clientes')
    }
  } catch (error) {
    console.error('Error:', error)
    alert('Error de conexión con el servidor')
  } finally {
    loading.value = false
  }
}

function nuevoCliente() {
  editando.value = false
  formCliente.value = { id: null, name: '', email: '' }
  mostrarFormulario.value = true
}

function editarCliente(cliente) {
  editando.value = true
  formCliente.value = { 
    id: cliente.id, 
    name: cliente.name, 
    email: cliente.email 
  }
  mostrarFormulario.value = true
}

async function guardarCliente() {
  if (!formCliente.value.name.trim()) {
    alert('El nombre es obligatorio')
    return
  }
  if (!formCliente.value.email.trim()) {
    alert('El email es obligatorio')
    return
  }
  if (!formCliente.value.email.includes('@') || !formCliente.value.email.includes('.')) {
    alert('El email no tiene un formato válido')
    return
  }

  loading.value = true
  try {
    let response
    
    if (editando.value) {
      response = await fetch(`https://localhost:7075/api/Clients/${formCliente.value.id}`, {
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: formCliente.value.name,
          email: formCliente.value.email
        })
      })
    } else {
      response = await fetch('https://localhost:7075/api/Clients', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({
          name: formCliente.value.name,
          email: formCliente.value.email
        })
      })
    }

    if (response.ok) {
      alert(editando.value ? 'Cliente actualizado correctamente' : 'Cliente creado correctamente')
      mostrarFormulario.value = false
      await obtenerClientes()
    } else {
      const error = await response.text()
      alert('Error: ' + error)
    }
  } catch (error) {
    console.error('Error:', error)
    alert('Error de conexión con el servidor')
  } finally {
    loading.value = false
  }
}

async function eliminarCliente(id, name) {
  if (!confirm(`¿Está seguro que desea eliminar al cliente "${name}"?`)) {
    return
  }

  try {
    const response = await fetch(`https://localhost:7075/api/Clients/${id}`, {
      method: 'DELETE'
    })

    if (response.ok) {
      alert('Cliente eliminado correctamente')
      await obtenerClientes()
    } else {
      const error = await response.text()
      alert('Error al eliminar: ' + error)
    }
  } catch (error) {
    console.error('Error:', error)
    alert('Error de conexión con el servidor')
  }
}

function cancelar() {
  mostrarFormulario.value = false
  formCliente.value = { id: null, name: '', email: '' }
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
    <h2>Gestion de Clientes</h2>

    <!-- Botones -->
    <div style="margin-bottom: 20px;">
      <button @click="nuevoCliente" style="padding: 8px 20px; background: #4CAF50; color: white; border: none; border-radius: 4px; cursor: pointer;">
        Agregar Cliente
      </button>
      <button @click="volver" style="margin-left: 10px; padding: 8px 20px; background: #f44336; color: white; border: none; border-radius: 4px; cursor: pointer;">
        Volver
      </button>
    </div>

    <!-- Formulario -->
    <div v-if="mostrarFormulario" style="margin-bottom: 20px; padding: 20px; border: 2px solid #4CAF50; border-radius: 8px; background: #f9f9f9;">
      <h3>{{ editando ? 'Editar Cliente' : 'Nuevo Cliente' }}</h3>
      
      <div style="margin-bottom: 10px;">
        <label style="display: inline-block; width: 80px;">Nombre:</label>
        <input type="text" v-model="formCliente.name" placeholder="Ej: Juan Perez" style="padding: 6px 10px; width: 250px; border: 1px solid #ccc; border-radius: 4px;" />
      </div>
      
      <div style="margin-bottom: 10px;">
        <label style="display: inline-block; width: 80px;">Email:</label>
        <input type="email" v-model="formCliente.email" placeholder="ejemplo@mail.com" style="padding: 6px 10px; width: 250px; border: 1px solid #ccc; border-radius: 4px;" />
      </div>
      
      <div>
        <button @click="guardarCliente" :disabled="loading" style="padding: 6px 20px; background: #4CAF50; color: white; border: none; border-radius: 4px; cursor: pointer;">
          {{ loading ? 'Guardando...' : 'Guardar' }}
        </button>
        <button @click="cancelar" style="margin-left: 10px; padding: 6px 20px; background: #999; color: white; border: none; border-radius: 4px; cursor: pointer;">
          Cancelar
        </button>
      </div>
    </div>

    <div v-if="loading && !mostrarFormulario">Cargando...</div>
    
    <div v-else-if="clients.length > 0">
      <p style="color: #666;">Total de clientes: {{ clients.length }}</p>
      
      <table border="1" cellpadding="8" cellspacing="0" style="width: 100%; border-collapse: collapse;">
        <thead style="background: #f0f0f0;">
          <tr>
            <th>ID</th>
            <th>Nombre</th>
            <th>Email</th>
            <th>Acciones</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="client in clients" :key="client.id" style="border-bottom: 1px solid #ddd;">
            <td style="text-align: center;">{{ client.id }}</td>
            <td>{{ client.name }}</td>
            <td>{{ client.email }}</td>
            <td style="text-align: center; white-space: nowrap;">
              <button @click="editarCliente(client)" style="margin: 2px; padding: 4px 12px; background: #FF9800; color: white; border: none; border-radius: 4px; cursor: pointer;">
                Editar
              </button>
              <button @click="eliminarCliente(client.id, client.name)" style="margin: 2px; padding: 4px 12px; background: #f44336; color: white; border: none; border-radius: 4px; cursor: pointer;">
                Eliminar
              </button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>

    <div v-else style="text-align: center; padding: 40px; color: #666;">
      <p>No hay clientes registrados</p>
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
</style>