using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("administrador")]
    public class Administrador
    {
        [Key]
        [Column("id_usuario")]
        public Guid IdUsuario { get; set; }

        // Relaciones
        [ForeignKey(nameof(IdUsuario))]
        public virtual Usuario Usuario { get; set; } = null!;
    }
}