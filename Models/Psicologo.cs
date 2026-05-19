using System.ComponentModel.DataAnnotations;

namespace Balance.API.Models
{
    public class Psicologo
    {
        [Key]
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellidos { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string NumeroColegiado { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}