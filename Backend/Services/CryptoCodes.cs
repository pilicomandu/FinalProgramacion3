namespace TrabajoFinalProgramacion3.Services
{
    /// <summary>
    /// Unifica los códigos de criptomonedas. Así "bitcoin", "btc" y "BTC"
    /// se tratan siempre como la misma moneda ("BTC"), que además es el
    /// código que espera la API de CriptoYa.
    /// </summary>
    public static class CryptoCodes
    {
        private static readonly Dictionary<string, string> Equivalencias = new()
        {
            { "btc", "BTC" },
            { "bitcoin", "BTC" },
            { "eth", "ETH" },
            { "ethereum", "ETH" },
            { "usdc", "USDC" }
        };

        public static string[] Soportadas => new[] { "BTC", "ETH", "USDC" };

        /// <summary>
        /// Devuelve el código unificado (BTC, ETH, USDC) o null si no es una cripto soportada.
        /// </summary>
        public static string? Normalizar(string? codigo)
        {
            if (string.IsNullOrWhiteSpace(codigo)) return null;

            return Equivalencias.TryGetValue(codigo.Trim().ToLower(), out var normalizado)
                ? normalizado
                : null;
        }

        /// <summary>
        /// Para datos viejos guardados con un código no reconocido: si no se puede
        /// normalizar, se devuelve en mayúsculas para no perderlo.
        /// </summary>
        public static string NormalizarOMantener(string codigo) =>
            Normalizar(codigo) ?? codigo.ToUpper();
    }
}
