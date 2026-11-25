using Microsoft.AspNetCore.Mvc;
using TrabajoFinalProgramacion3.Backend.DTOs;
using TrabajoFinalProgramacion3.Controllers;
using TrabajoFinalProgramacion3.Data;
using TrabajoFinalProgramacion3.DTOs;
using TrabajoFinalProgramacion3.Models;

namespace Backend.Tests
{
    public class TransactionsControllerTests
    {
        // Precios falsos de CriptoYa usados en los tests
        private const decimal ASK = 1000m; // precio al que el cliente compra
        private const decimal BID = 900m;  // precio al que el cliente vende

        private static (TransactionsController controller, AppDbContext db, List<string> urls) Crear()
        {
            var db = Helpers.CrearContexto();
            db.Clients.Add(new Client { Id = 1, Name = "Juan Perez", Email = "juan@mail.com" });
            db.SaveChanges();

            var urls = new List<string>();
            var controller = new TransactionsController(db, Helpers.CriptoYaFalso(ASK, BID, urls));
            return (controller, db, urls);
        }

        private static CreateTransactionDTO Dto(string accion = "purchase", decimal cantidad = 2m, string crypto = "usdc", int clientId = 1) => new()
        {
            CryptoCode = crypto,
            Action = accion,
            ClientId = clientId,
            CryptoAmount = cantidad,
            DateTime = new DateTime(2026, 11, 25, 17, 50, 0)
        };

        // ---------- Alta de compra ----------

        [Fact]
        public async Task Compra_CalculaElMontoConElPrecioDeCompra()
        {
            var (controller, db, _) = Crear();

            var resultado = await controller.Create(Dto("purchase", 2m));

            var ok = Assert.IsType<OkObjectResult>(resultado);
            var tx = Assert.IsType<Transaction>(ok.Value);
            Assert.Equal(2m * ASK, tx.Money);
            Assert.Equal(1, db.Transactions.Count());
        }

        [Fact]
        public async Task Compra_GuardaElCodigoUnificadoYConsultaCriptoYaConEl()
        {
            var (controller, db, urls) = Crear();

            await controller.Create(Dto(crypto: "bitcoin", cantidad: 0.001m));

            Assert.Equal("BTC", db.Transactions.Single().CryptoCode);
            Assert.StartsWith("https://criptoya.com/api/satoshitango/BTC/ars/", urls.Single());
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task Compra_RechazaCantidadNoPositiva(decimal cantidad)
        {
            var (controller, db, urls) = Crear();

            var resultado = await controller.Create(Dto(cantidad: cantidad));

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Empty(db.Transactions);
            Assert.Empty(urls); // ni siquiera consulta el precio
        }

        [Fact]
        public async Task Compra_RechazaAccionInvalida()
        {
            var (controller, db, _) = Crear();

            var resultado = await controller.Create(Dto(accion: "robo"));

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Empty(db.Transactions);
        }

        [Fact]
        public async Task Compra_RechazaCriptoNoSoportada()
        {
            var (controller, db, _) = Crear();

            var resultado = await controller.Create(Dto(crypto: "doge"));

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Empty(db.Transactions);
        }

        [Fact]
        public async Task Compra_RechazaClienteInexistente()
        {
            var (controller, db, _) = Crear();

            var resultado = await controller.Create(Dto(clientId: 99));

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Empty(db.Transactions);
        }

        // ---------- Alta de venta ----------

        [Fact]
        public async Task Venta_ConSaldoSuficiente_UsaElPrecioDeVenta()
        {
            var (controller, _, _) = Crear();
            await controller.Create(Dto("purchase", 3m));

            var resultado = await controller.Create(Dto("sale", 2m));

            var tx = Assert.IsType<Transaction>(Assert.IsType<OkObjectResult>(resultado).Value);
            Assert.Equal(2m * BID, tx.Money);
        }

        [Fact]
        public async Task Venta_NoPermiteVenderMasDeLoQueSeTiene()
        {
            // Ejemplo de la consigna: compré 2 USDC e intento vender 3
            var (controller, db, _) = Crear();
            await controller.Create(Dto("purchase", 2m));

            var resultado = await controller.Create(Dto("sale", 3m));

            Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(1, db.Transactions.Count());
        }

        [Fact]
        public async Task Venta_PermiteVenderTodoElSaldo()
        {
            var (controller, _, _) = Crear();
            await controller.Create(Dto("purchase", 2m));

            var resultado = await controller.Create(Dto("sale", 2m));

            Assert.IsType<OkObjectResult>(resultado);
        }

        // ---------- Lectura, edición y borrado ----------

        [Fact]
        public async Task GetByClient_DevuelveLasTransaccionesOrdenadasPorFechaDescendente()
        {
            var (controller, _, _) = Crear();
            var vieja = Dto(); vieja.DateTime = new DateTime(2026, 1, 1);
            var nueva = Dto(); nueva.DateTime = new DateTime(2026, 6, 1);
            await controller.Create(vieja);
            await controller.Create(nueva);

            var lista = Assert.IsType<List<Transaction>>(Assert.IsType<OkObjectResult>(controller.GetByClient(1)).Value);

            Assert.Equal(2, lista.Count);
            Assert.True(lista[0].DateTime > lista[1].DateTime);
        }

        [Fact]
        public void GetById_DevuelveNotFoundSiNoExiste()
        {
            var (controller, _, _) = Crear();
            Assert.IsType<NotFoundObjectResult>(controller.GetById(123));
        }

        [Fact]
        public async Task Patch_ModificaSoloLosCamposEnviados()
        {
            var (controller, db, _) = Crear();
            await controller.Create(Dto("purchase", 2m));
            var id = db.Transactions.Single().Id;

            controller.Update(id, new UpdateTransactionDTO { Money = 170.55m });

            var tx = db.Transactions.Single();
            Assert.Equal(170.55m, tx.Money);
            Assert.Equal(2m, tx.CryptoAmount); // no cambió
            Assert.Equal("purchase", tx.Action);
        }

        [Fact]
        public async Task Delete_BorraLaTransaccion()
        {
            var (controller, db, _) = Crear();
            await controller.Create(Dto());
            var id = db.Transactions.Single().Id;

            var resultado = controller.Delete(id);

            Assert.IsType<NoContentResult>(resultado);
            Assert.Empty(db.Transactions);
        }

        // ---------- Análisis de cartera ----------

        [Fact]
        public async Task Estado_SoloMuestraCriptosConSaldoYLasValuaAlPrecioDeVenta()
        {
            // Ejemplo de la consigna: se compró y vendió el mismo USDC, y quedan 0.03 BTC
            var (controller, _, _) = Crear();
            await controller.Create(Dto("purchase", 1.01m, "usdc"));
            await controller.Create(Dto("sale", 1.01m, "usdc"));
            await controller.Create(Dto("purchase", 0.01m, "bitcoin"));
            await controller.Create(Dto("purchase", 0.02m, "btc"));

            var json = Helpers.ComoJson(await controller.GetEstadoCartera(1));

            var criptos = json.GetProperty("crypto");
            Assert.Equal(1, criptos.GetArrayLength());
            Assert.Equal("BTC", criptos[0].GetProperty("cryptoCode").GetString());
            Assert.Equal(0.03m, criptos[0].GetProperty("cantidad").GetDecimal());
            Assert.Equal(0.03m * BID, json.GetProperty("total").GetDecimal());
        }

        [Fact]
        public async Task Estado_ClienteSinCriptos_DevuelveListaVaciaYTotalCero()
        {
            var (controller, _, _) = Crear();

            var json = Helpers.ComoJson(await controller.GetEstadoCartera(1));

            Assert.Equal(0, json.GetProperty("crypto").GetArrayLength());
            Assert.Equal(0, json.GetProperty("total").GetDecimal());
        }

        [Fact]
        public async Task Estado_ClienteInexistente_DevuelveNotFound()
        {
            var (controller, _, _) = Crear();
            Assert.IsType<NotFoundObjectResult>(await controller.GetEstadoCartera(99));
        }
    }
}
