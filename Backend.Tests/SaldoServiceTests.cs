using TrabajoFinalProgramacion3.Models;
using TrabajoFinalProgramacion3.Services;

namespace Backend.Tests
{
    public class SaldoServiceTests
    {
        private static Transaction Tx(int clientId, string crypto, string accion, decimal cantidad) => new()
        {
            ClientId = clientId,
            CryptoCode = crypto,
            Action = accion,
            CryptoAmount = cantidad,
            Money = 0,
            DateTime = new DateTime(2026, 11, 25)
        };

        [Fact]
        public void SumaComprasYRestaVentas()
        {
            using var db = Helpers.CrearContexto();
            db.Transactions.AddRange(
                Tx(1, "BTC", "purchase", 0.01m),
                Tx(1, "BTC", "purchase", 0.02m),
                Tx(1, "BTC", "sale", 0.005m));
            db.SaveChanges();

            var saldo = new SaldoService(db).ObtenerSaldo(1, "BTC");

            Assert.Equal(0.025m, saldo);
        }

        [Fact]
        public void SoloCuentaLaCriptoYElClientePedidos()
        {
            using var db = Helpers.CrearContexto();
            db.Transactions.AddRange(
                Tx(1, "BTC", "purchase", 1m),
                Tx(1, "USDC", "purchase", 50m),  // otra cripto
                Tx(2, "BTC", "purchase", 7m));   // otro cliente
            db.SaveChanges();

            Assert.Equal(1m, new SaldoService(db).ObtenerSaldo(1, "BTC"));
        }

        [Fact]
        public void TrataComoLaMismaCriptoLosCodigosEquivalentes()
        {
            using var db = Helpers.CrearContexto();
            db.Transactions.AddRange(
                Tx(1, "eth", "purchase", 1m),
                Tx(1, "ethereum", "purchase", 2m),
                Tx(1, "ETH", "sale", 0.5m));
            db.SaveChanges();

            Assert.Equal(2.5m, new SaldoService(db).ObtenerSaldo(1, "ETH"));
        }

        [Fact]
        public void DevuelveCeroSiNoHayTransacciones()
        {
            using var db = Helpers.CrearContexto();
            Assert.Equal(0m, new SaldoService(db).ObtenerSaldo(1, "BTC"));
        }
    }
}
