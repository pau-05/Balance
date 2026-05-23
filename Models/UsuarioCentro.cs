using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("usuario_centro")]
    [PrimaryKey(nameof(IdUsuario), nameof(IdCentro), nameof(IdRol))]
    public class UsuarioCentro
    {
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        [Column("id_centro")]
        public Guid IdCentro { get; set; }

        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("activo")]
        public bool Activo { get; set; } = true;

        [Column("fecha_asignacion")]
        public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;

        // Propiedades de navegación
        [ForeignKey(nameof(IdUsuario))]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(IdCentro))]
        public virtual Centro Centro { get; set; } = null!;

        [ForeignKey(nameof(IdRol))]
        public virtual Rol Rol { get; set; } = null!;

        // Helper para usar enum en código
        [NotMapped]
        public RolUsuario RolEnum
        {
            get => RolUsuarioExtensions.FromString(Rol?.Nombre ?? "PACIENTE");
            set => IdRol = (int)value;
        }
    }
}