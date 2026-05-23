using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance.API.Models
{
    [Table("recurso")]
    public class Recurso
    {
        [Key]
        [Column("id_recurso")]
        public Guid IdRecurso { get; set; }

        [Column("nombre")]
        public string Nombre { get; set; } = string.Empty;

        [Column("tipo")]
        public string Tipo { get; set; } = string.Empty; // PDF, VIDEO, ENLACE, PLANTILLA

        [Column("url_almacenamiento")]
        public string UrlAlmacenamiento { get; set; } = string.Empty;

        [Column("tamanio_bytes")]
        public long? TamanioBytes { get; set; }

        [Column("fecha_subida")]
        public DateTime FechaSubida { get; set; } = DateTime.UtcNow;

        [Column("fecha_actualizacion")]
        public DateTime? FechaActualizacion { get; set; }

        [Column("subido_por")]
        public Guid SubidoPor { get; set; }

        [Column("id_centro")]
        public Guid IdCentro { get; set; }

        [ForeignKey(nameof(SubidoPor))]
        public virtual Usuario Usuario { get; set; } = null!;

        [ForeignKey(nameof(IdCentro))]
        public virtual Centro Centro { get; set; } = null!;
    }
}