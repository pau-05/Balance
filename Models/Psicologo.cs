using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("Psicologos")]
    public class Psicologo
    {
        [Key]
        [Column("Id")]
        public Guid Id { get; set; }

        [Column("Nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("Apellidos")]
        public string Apellidos { get; set; } = string.Empty;

        [Column("Email")]
        public string Email { get; set; } = string.Empty;

        [Column("NumeroColegiado")]
        public string NumeroColegiado { get; set; } = string.Empty;

        [Column("FechaRegistro")]
        public DateTime FechaRegistro { get; set; }
    }
}