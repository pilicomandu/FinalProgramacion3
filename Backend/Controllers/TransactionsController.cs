using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Net.Http.Json;
using System.Text.Json;
using TrabajoFinalProgramacion3.Backend.DTOs;
using TrabajoFinalProgramacion3.Data;
using TrabajoFinalProgramacion3.DTOs;
using TrabajoFinalProgramacion3.Models;
using TrabajoFinalProgramacion3.Services;

namespace TrabajoFinalProgramacion3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HttpClient _httpClient;

        public TransactionsController(AppDbContext context, HttpClient httpClient)
        {
            _context = context;
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateTransactionDTO transactionDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (transactionDTO.CryptoAmount <= 0)
            {
                return BadRequest("La cantidad de criptomonedas debe ser mayor a 0.");
            }

            var accion = transactionDTO.Action.ToLower();
            if (accion != "purchase" && accion != "sale")
            {
                return BadRequest("La acción debe ser 'purchase' (compra) o 'sale' (venta).");
            }
            transactionDTO.Action = accion;

            var cryptosValidas = new[] { "btc", "eth", "usdc", "bitcoin", "ethereum" };
            if (!cryptosValidas.Contains(transactionDTO.CryptoCode.ToLower()))
            {
                return BadRequest($"La criptomoneda '{transactionDTO.CryptoCode}' no es válida. Opciones: BTC, ETH, USDC.");
            }

            var clientExists = _context.Clients.Any(c => c.Id == transactionDTO.ClientId);

            if (!clientExists)
            {
                return BadRequest("El cliente seleccionado no existe.");
            }

            if (string.Equals(transactionDTO.Action, "sale", StringComparison.OrdinalIgnoreCase))
            {
                var saldoService = new SaldoService(_context);
                var saldoActual = saldoService.ObtenerSaldo(
                    transactionDTO.ClientId,
                    transactionDTO.CryptoCode
                );

                if (saldoActual < transactionDTO.CryptoAmount)
                {
                    return BadRequest($"No se puede vender {transactionDTO.CryptoAmount} {transactionDTO.CryptoCode}. " +
                                     $"Saldo disponible: {saldoActual} {transactionDTO.CryptoCode}");
                }
            }

            var crypto = transactionDTO.CryptoCode.ToUpper();

            var cryptoMap = new Dictionary<string, string>
            {
                { "bitcoin", "BTC" },
                { "ethereum", "ETH" },
                { "usdc", "USDC" },
                { "btc", "BTC" },
                { "eth", "ETH" }
            };

            if (cryptoMap.ContainsKey(crypto.ToLower()))
            {
                crypto = cryptoMap[crypto.ToLower()];
            }

            var amountStr = transactionDTO.CryptoAmount.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var url = $"https://criptoya.com/api/satoshitango/{crypto}/ars/{amountStr}";

            Console.WriteLine("URL CRIPTOYA: " + url);

            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                return StatusCode(
                    StatusCodes.Status502BadGateway,
                    $"No se pudo obtener el precio desde CriptoYa (código {(int)response.StatusCode})."
                );
            }

            var priceData = await response.Content.ReadFromJsonAsync<CriptoPrice>(options);
            if (priceData == null)
            {
                return BadRequest("No se pudo obtener el precio de la criptomoneda.");
            }

            decimal price;

            if (string.Equals(transactionDTO.Action, "purchase", StringComparison.OrdinalIgnoreCase))
            {
                price = priceData.Ask;
            }
            else
            {
                price = priceData.Bid;
            }

            decimal totalMoney = price * transactionDTO.CryptoAmount;

            var transaction = new Transaction
            {
                CryptoCode = transactionDTO.CryptoCode,
                Action = transactionDTO.Action,
                ClientId = transactionDTO.ClientId,
                CryptoAmount = transactionDTO.CryptoAmount,
                DateTime = transactionDTO.DateTime,
                Money = totalMoney
            };

            _context.Transactions.Add(transaction);
            await _context.SaveChangesAsync();

            return Ok(transaction);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var transaction = _context.Transactions.Find(id);

            if (transaction == null)
            {
                return NotFound();
            }

            _context.Transactions.Remove(transaction);
            _context.SaveChanges();

            return NoContent();
        }

        [HttpGet("client/{clientId}")]
        public IActionResult GetByClient(int clientId)
        {
            var transactions = _context.Transactions
                .Where(t => t.ClientId == clientId)
                .OrderByDescending(t => t.DateTime)
                .ToList();

            return Ok(transactions);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var transaction = _context.Transactions
                .FirstOrDefault(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound($"No se encontró la transacción con ID {id}");
            }

            return Ok(transaction);
        }

        [HttpPatch("{id}")]
        public IActionResult Update(int id, UpdateTransactionDTO updateDTO)
        {
            var transaction = _context.Transactions.FirstOrDefault(t => t.Id == id);

            if (transaction == null)
            {
                return NotFound($"No se encontró la transacción con ID {id}");
            }

            if (!string.IsNullOrEmpty(updateDTO.CryptoCode))
                transaction.CryptoCode = updateDTO.CryptoCode;

            if (!string.IsNullOrEmpty(updateDTO.Action))
                transaction.Action = updateDTO.Action;

            if (updateDTO.ClientId.HasValue)
                transaction.ClientId = updateDTO.ClientId.Value;

            if (updateDTO.CryptoAmount.HasValue)
                transaction.CryptoAmount = updateDTO.CryptoAmount.Value;

            if (updateDTO.Money.HasValue)
                transaction.Money = updateDTO.Money.Value;

            if (updateDTO.DateTime.HasValue)
                transaction.DateTime = updateDTO.DateTime.Value;

            _context.SaveChanges();

            return Ok(transaction);
        }

        [HttpGet("estado/{clientId}")]
        public async Task<IActionResult> GetEstadoCartera(int clientId)
        {
            var client = _context.Clients.FirstOrDefault(c => c.Id == clientId);
            if (client == null)
            {
                return NotFound($"No se encontró el cliente con ID {clientId}");
            }

            var transactions = _context.Transactions
                .Where(t => t.ClientId == clientId)
                .ToList();

            var saldos = new Dictionary<string, decimal>();

            foreach (var t in transactions)
            {
                if (!saldos.ContainsKey(t.CryptoCode))
                {
                    saldos[t.CryptoCode] = 0;
                }

                if (string.Equals(t.Action, "purchase", StringComparison.OrdinalIgnoreCase))
                {
                    saldos[t.CryptoCode] += t.CryptoAmount;
                }

                else if (string.Equals(t.Action, "sale", StringComparison.OrdinalIgnoreCase))
                {
                    saldos[t.CryptoCode] -= t.CryptoAmount;
                }
            }

            saldos = saldos.Where(s => s.Value > 0).ToDictionary(s => s.Key, s => s.Value);

            if (saldos.Count == 0)
            {
                return Ok(new
                {
                    clientName = client.Name,
                    clientEmail = client.Email,
                    crypto = new List<object>(),
                    total = 0,
                    message = "Este cliente no tiene criptomonedas en su cartera"
                });
            }

            var resultado = new List<object>();
            decimal totalGeneral = 0;

            foreach (var saldo in saldos)
            {
                try
                {
                    var cryptoCode = saldo.Key.ToUpper();
                    var url = $"https://criptoya.com/api/satoshitango/{cryptoCode}/ars/1";

                    var response = await _httpClient.GetAsync(url);
                    if (response.IsSuccessStatusCode)
                    {
                        var priceData = await response.Content.ReadFromJsonAsync<CriptoPrice>();
                        if (priceData != null)
                        {
                            // Se valúa la cartera al precio de venta (bid): es lo que
                            // el cliente recibiría hoy si vendiera sus criptomonedas.
                            var precioActual = priceData.Bid;
                            var valorTotal = saldo.Value * precioActual;
                            totalGeneral += valorTotal;

                            resultado.Add(new
                            {
                                cryptoCode = saldo.Key,
                                cantidad = saldo.Value,
                                precioUnitario = precioActual,
                                valorTotal = valorTotal
                            });
                        }
                    }
                    else
                    {
                        resultado.Add(new
                        {
                            cryptoCode = saldo.Key,
                            cantidad = saldo.Value,
                            precioUnitario = 0,
                            valorTotal = 0,
                            error = "No se pudo obtener el precio"
                        });
                    }
                }
                catch (Exception ex)
                {
                    resultado.Add(new
                    {
                        cryptoCode = saldo.Key,
                        cantidad = saldo.Value,
                        precioUnitario = 0,
                        valorTotal = 0,
                        error = ex.Message
                    });
                }
            }

            return Ok(new
            {
                clientName = client.Name,
                clientEmail = client.Email,
                crypto = resultado,
                total = totalGeneral
            });
        }
    }
}