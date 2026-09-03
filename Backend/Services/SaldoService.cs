using TrabajoFinalProgramacion3.Data;
using TrabajoFinalProgramacion3.Models;

namespace TrabajoFinalProgramacion3.Backend.Services
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
            var transactions = _context.Transactions
                .Where(t => t.ClientId == clientId && t.CryptoCode.ToLower() == cryptoCode.ToLower())
                .ToList();

            decimal saldo = 0;

            foreach (var t in transactions)
            {
                if (t.Action.ToLower() == "purchase")
                {
                    saldo += t.CryptoAmount;
                }
                else if (t.Action.ToLower() == "sale")
                {
                    saldo -= t.CryptoAmount;
                }
            }

            return saldo;
        }
    }
}