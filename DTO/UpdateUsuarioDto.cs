using System.Text.Json;

namespace Balance.API.DTO
{
    //DTO para actualizar los datos básicos de un usuario
    public class UpdateUsuarioDto
    {
        public string Nombre { get; set; }
        public string Ape1 { get; set; }
        public string Ape2 { get; set; }
        public string Email { get; set; }
        public string Rol { get; set; }
        public DateTime? FechaNacimiento { get; set; }
        //public DateTime? FechaAlta { get; set; }
        //public DateTime? FechaBaja { get; set; }
        public string Telefono { get; set; }
        public string Direccion { get; set; }
        public string NumLicencia { get; set; }
        public string HorarioJson { get; set; }
    }
}
