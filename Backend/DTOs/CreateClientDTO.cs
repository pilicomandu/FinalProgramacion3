using System.ComponentModel.DataAnnotations;

namespace TrabajoFinalProgramacion3.DTOs
{
    public class CreateClientDTO
    {
        [Required]
        public string Name { get; set; } = "";

        [Required]
        [EmailAddress]
        public string Email { get; set; } = "";
    }
}
