using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TrabajoFinalProgramacion3.Backend.DTOs
{
    public class CreateTransactionDTO
    {
        [Required]
        [JsonPropertyName("cryptoCode")]
        public string CryptoCode { get; set; } = "";

        [Required]
        [JsonPropertyName("action")]
        public string Action { get; set; } = "";

        [Required]
        [JsonPropertyName("clientId")]
        public int ClientId { get; set; }

        [Required]
        [JsonPropertyName("cryptoAmount")]
        public decimal CryptoAmount { get; set; }

        [Required]
        [JsonPropertyName("dateTime")]
        public DateTime DateTime { get; set; }
    }
}
 