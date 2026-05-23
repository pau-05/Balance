using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("roles")]
    public class Rol
    {
        [Key]
        [Column("id_rol")]
        public int IdRol { get; set; }

        [Column("nombre_rol")]
        public string Nombre { get; set; } = string.Empty;

        // Relaciones
        public virtual ICollection<UsuarioCentro> UsuariosCentro { get; set; } = new List<UsuarioCentro>();
    }
}
