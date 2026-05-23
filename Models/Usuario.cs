using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("usuario")]
    public class Usuario
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("ape1")]
        public string Ape1 { get; set; } = string.Empty;

        [Column("ape2")]
        public string? Ape2 { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Column("fecha_registro")]
        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

        [Column("fecha_baja")]
        public DateTime? FechaBaja { get; set; }

        // Relaciones
        public virtual ICollection<UsuarioCentro> UsuariosCentro { get; set; } = new List<UsuarioCentro>();
        public virtual Paciente? PacienteDatos { get; set; }
        public virtual Psicologo? PsicologoDatos { get; set; }
        public virtual Administrador? AdministradorDatos { get; set; }
    }
}