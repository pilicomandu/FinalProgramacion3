using System.ComponentModel.DataAnnotations;

namespace TrabajoFinalProgramacion3.Models
{
    public class Client 
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";

        public List<Transaction> Transactions { get; set; } = new();
        
    }
}
