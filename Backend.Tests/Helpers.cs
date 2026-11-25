using System.Net;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TrabajoFinalProgramacion3.Data;

namespace Backend.Tests
{
    /// <summary>
    /// Utilidades compartidas por los tests.
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Crea una base de datos en memoria, nueva y vacía para cada test
        /// (así los tests no dependen de SQL Server ni se pisan entre sí).
        /// </summary>
        public static AppDbContext CrearContexto()
        {
            var opciones = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new AppDbContext(opciones);
        }

        /// <summary>
        /// HttpClient falso que responde como CriptoYa con los precios indicados,
        /// sin salir a internet. Guarda las URLs consultadas para poder verificarlas.
        /// </summary>
        public static HttpClient CriptoYaFalso(decimal ask, decimal bid, List<string>? urlsConsultadas = null)
        {
            var json = JsonSerializer.Serialize(new { ask, bid, totalAsk = ask, totalBid = bid, time = 0 });
            return new HttpClient(new HandlerFalso(json, HttpStatusCode.OK, urlsConsultadas));
        }

        /// <summary>
        /// Convierte el objeto devuelto por un endpoint (OkObjectResult) a JSON,
        /// útil para leer los objetos anónimos que arma el controller.
        /// </summary>
        public static JsonElement ComoJson(IActionResult resultado)
        {
            var ok = Assert.IsType<OkObjectResult>(resultado);
            return JsonSerializer.SerializeToElement(ok.Value);
        }

        private class HandlerFalso : HttpMessageHandler
        {
            private readonly string _json;
            private readonly HttpStatusCode _status;
            private readonly List<string>? _urls;

            public HandlerFalso(string json, HttpStatusCode status, List<string>? urls)
            {
                _json = json;
                _status = status;
                _urls = urls;
            }

            protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                _urls?.Add(request.RequestUri!.ToString());
                return Task.FromResult(new HttpResponseMessage(_status)
                {
                    Content = new StringContent(_json, Encoding.UTF8, "application/json")
                });
            }
        }
    }
}
