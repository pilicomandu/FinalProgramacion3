# Trabajo Final – Programación III – Cartera de Criptomonedas

Sistema para que un asesor registre las compras y ventas de criptomonedas de sus clientes
y consulte el estado actual de cada cartera, con precios en tiempo real de [CriptoYa](https://criptoya.com) (exchange: SatoshiTango).

Criptomonedas soportadas: **BTC (Bitcoin), ETH (Ethereum) y USDC**.

## Tecnologías

| Capa       | Tecnología                                   |
|------------|----------------------------------------------|
| Frontend   | Vue.js 3 + Vue Router + Vite                 |
| Backend    | ASP.NET Core 8 (Web API) + Entity Framework  |
| Base datos | SQL Server Express                           |

## Cómo levantarlo

### 1. Backend

Requisitos: .NET 8 SDK y SQL Server Express (instancia `localhost\SQLEXPRESS`).
La cadena de conexión está en `Backend/appsettings.json`.

```bash
cd Backend
dotnet ef database update      # crea la base y las tablas (solo la primera vez)
dotnet run --launch-profile https
```

La API queda en `https://localhost:7075` y Swagger en `https://localhost:7075/swagger`.

### 2. Frontend

Requisitos: Node.js 20.19+ o 22.12+.

```bash
cd Frontend
npm install
npm run dev
```

Abrir `http://localhost:5173`.

## Endpoints

| Método | Ruta                                  | Descripción                                        |
|--------|---------------------------------------|----------------------------------------------------|
| GET    | `/api/Clients`                        | Lista de clientes                                  |
| GET    | `/api/Clients/{id}`                   | Un cliente                                         |
| POST   | `/api/Clients`                        | Alta de cliente (`name`, `email`)                  |
| PUT    | `/api/Clients/{id}`                   | Edición de cliente                                 |
| DELETE | `/api/Clients/{id}`                   | Baja de cliente (si no tiene transacciones)        |
| POST   | `/api/Transactions`                   | Alta de compra/venta (calcula el monto con CriptoYa) |
| GET    | `/api/Transactions/client/{clientId}` | Historial de un cliente, ordenado por fecha        |
| GET    | `/api/Transactions/{id}`              | Una transacción                                    |
| PATCH  | `/api/Transactions/{id}`              | Edición cruda de una transacción                   |
| DELETE | `/api/Transactions/{id}`              | Borrado de una transacción                         |
| GET    | `/api/Transactions/estado/{clientId}` | Estado actual de la cartera valuada en ARS         |

Ejemplo de alta de transacción:

```json
{
  "cryptoCode": "BTC",
  "action": "purchase",
  "clientId": 2,
  "cryptoAmount": 0.0007,
  "dateTime": "2026-11-25T17:50"
}
```

## Reglas de negocio

- La cantidad debe ser mayor a 0, la acción `purchase` o `sale` y la cripto una de las soportadas.
- **Compra:** el monto se calcula con el precio `ask` (lo que cobra el exchange al vender).
- **Venta:** se valida que el cliente tenga saldo suficiente de esa cripto y el monto se calcula con el precio `bid`.
- **Análisis de cartera:** suma compras y resta ventas por cripto; muestra solo las que tienen saldo positivo, valuadas al precio `bid` actual.
