using System.ComponentModel.DataAnnotations;

namespace TrabajoFinalProgramacion3.Models
{
    public class Transaction
    {
        public int Id { get; set; }
        public required string CryptoCode { get; set; }
        public required string Action { get; set; }
        [Required]
        public int ClientId { get; set; }
        public Client? Client { get; set; }
        [Required]
        public decimal CryptoAmount { get; set; }
        [Required]
        public decimal Money { get; set; }
        [Required]
        public DateTime DateTime { get; set; }

    }
}
