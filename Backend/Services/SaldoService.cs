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

        public decimal ObtenerSaldo(int clientId, string cryptoCode)
        {
            var cryptoUpper = cryptoCode.ToUpperInvariant();


            var transactions = _context.Transactions
                .Where(t => t.ClientId == clientId)
                .ToList()
                .Where(t => string.Equals(t.CryptoCode, cryptoUpper, StringComparison.OrdinalIgnoreCase))
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