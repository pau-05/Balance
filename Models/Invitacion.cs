using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("invitaciones")]
    public class Invitacion
    {
        [Key]
        [Column("id")]
        public Guid Id { get; set; }

        [Column("email")]
        public string Email { get; set; } = string.Empty;

        [Column("codigo")]
        public string Codigo { get; set; } = string.Empty;

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("id_centro")]
        public Guid IdCentro { get; set; }

        [Column("creado_por")]
        public Guid CreadoPor { get; set; }

        [Column("creado_en")]
        public DateTime CreadoEn { get; set; } = DateTime.UtcNow;

        [Column("expira_en")]
        public DateTime ExpiraEn { get; set; }

        [Column("usado_en")]
        public DateTime? UsadoEn { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        // Propiedades de navegación
        [ForeignKey(nameof(IdRol))]
        public virtual Rol Rol { get; set; } = null!;

        [ForeignKey(nameof(IdCentro))]
        public virtual Centro Centro { get; set; } = null!;

        [ForeignKey(nameof(CreadoPor))]
        public virtual Usuario Creador { get; set; } = null!;

        // Helper
        [NotMapped]
        public RolUsuario RolEnum
        {
            get => RolUsuarioExtensions.FromString(Rol?.Nombre ?? "PACIENTE");
            set => IdRol = (int)value;
        }
    }
}