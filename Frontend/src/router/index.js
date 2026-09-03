import { createRouter, createWebHistory } from 'vue-router'
import Home from '../views/Home.vue'
import Historial from '../views/Historial.vue'
import DetalleTransaccion from '../views/DetalleTransaccion.vue'
import EditarTransaccion from '../views/EditarTransaccion.vue'
import Analisis from '../views/Analisis.vue'
import Clientes from '../views/Clientes.vue'  

const routes = [
  {
    path: '/',
    name: 'Home',
    component: Home
  },
  {
    path: '/historial',
    name: 'Historial',
    component: Historial
  },
  {
    path: '/transaccion/:id',
    name: 'DetalleTransaccion',
    component: DetalleTransaccion
  },
  {
    path: '/transaccion/:id/editar',
    name: 'EditarTransaccion',
    component: EditarTransaccion
  },
  {
    path: '/analisis',
    name: 'Analisis',
    component: Analisis
  },
  {
    path: '/clientes',  
    name: 'Clientes',
    component: Clientes
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router