using TrabajoFinalProgramacion3.Services;

namespace Backend.Tests
{
    public class CryptoCodesTests
    {
        [Theory]
        [InlineData("btc", "BTC")]
        [InlineData("BTC", "BTC")]
        [InlineData("bitcoin", "BTC")]
        [InlineData("eth", "ETH")]
        [InlineData("Ethereum", "ETH")]
        [InlineData("usdc", "USDC")]
        [InlineData(" usdc ", "USDC")]
        public void Normalizar_DevuelveElCodigoUnificado(string entrada, string esperado)
        {
            Assert.Equal(esperado, CryptoCodes.Normalizar(entrada));
        }

        [Theory]
        [InlineData("doge")]
        [InlineData("")]
        [InlineData(null)]
        public void Normalizar_DevuelveNullSiNoEsSoportada(string? entrada)
        {
            Assert.Null(CryptoCodes.Normalizar(entrada));
        }

        [Fact]
        public void NormalizarOMantener_ConservaCodigosDesconocidosEnMayusculas()
        {
            Assert.Equal("DOGE", CryptoCodes.NormalizarOMantener("doge"));
            Assert.Equal("ETH", CryptoCodes.NormalizarOMantener("ethereum"));
        }
    }
}
