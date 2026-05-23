using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("centro")]
    public class Centro
    {
        [Key]
        [Column("id_centro")]
        public Guid IdCentro { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("direccion")]
        public string? Direccion { get; set; }

        [Column("codigo_postal")]
        public string? CodigoPostal { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("telefono")]
        public string? Telefono { get; set; }

        [Column("plan_suscripcion")]
        public string PlanSuscripcion { get; set; } = "Basico";

        [Column("tipo_pago")]
        public string TipoPago { get; set; } = "Mensual";

        [Column("fecha_contratacion")]
        public DateTime FechaContratacion { get; set; } = DateTime.UtcNow;

        [Column("fecha_renovacion")]
        public DateTime FechaRenovacion { get; set; }

        // Relaciones
        public virtual ICollection<UsuarioCentro> UsuariosCentro { get; set; } = new List<UsuarioCentro>();
        public virtual ICollection<Invitacion> Invitaciones { get; set; } = new List<Invitacion>();
    }
}