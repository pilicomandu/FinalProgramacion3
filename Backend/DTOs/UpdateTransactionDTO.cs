namespace TrabajoFinalProgramacion3.DTOs
{
    public class UpdateTransactionDTO
    {
        public string? CryptoCode { get; set; }
        public string? Action { get; set; }
        public int? ClientId { get; set; }
        public decimal? CryptoAmount { get; set; }
        public decimal? Money { get; set; }
        public DateTime? DateTime { get; set; }
    }
}