using TrabajoFinalProgramacion3.Data;

namespace TrabajoFinalProgramacion3.Services
{
    public class SaldoService
    {
        private readonly AppDbContext _context;

        public SaldoService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calcula cuánto tiene un cliente de una cripto: suma las compras y resta las ventas.
        /// </summary>
        public decimal ObtenerSaldo(int clientId, string cryptoCode)
        {
            var codigo = CryptoCodes.NormalizarOMantener(cryptoCode);

            // Se traen las transacciones del cliente y se filtra en memoria por el
            // código unificado, así "eth" y "ethereum" cuentan como la misma moneda.
            var transactions = _context.Transactions
                .Where(t => t.ClientId == clientId)
                .ToList()
                .Where(t => CryptoCodes.NormalizarOMantener(t.CryptoCode) == codigo)
                .ToList();

            decimal saldo = 0;

            foreach (var t in transactions)
            {
                if (string.Equals(t.Action, "purchase", StringComparison.OrdinalIgnoreCase))
                    saldo += t.CryptoAmount;
                else if (string.Equals(t.Action, "sale", StringComparison.OrdinalIgnoreCase))
                    saldo -= t.CryptoAmount;
            }

            return saldo;
        }
    }
}
