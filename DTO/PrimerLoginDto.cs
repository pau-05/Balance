namespace Balance.API.DTO
{
    public class VerificarCodigoDto
    {
        public string Codigo { get; set; } = string.Empty;
    }

    public class CodigoValidoResponseDto
    {
        public bool Valido { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Rol { get; set; } = string.Empty;
    }

    public class PrimerLoginDto
    {
        // Datos de invitación
        public string Codigo { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;

        // Datos personales
        public string Nombre { get; set; } = string.Empty;
        public string Ape1 { get; set; } = string.Empty;
        public string? Ape2 { get; set; }
        public string Password { get; set; } = string.Empty;

        // Datos específicos según rol (serán validados según el Rol de la invitación)
        public DateTime? FechaNacimiento { get; set; }
        public string? Telefono { get; set; }
        public string? Direccion { get; set; }
        public string? NumLicencia { get; set; }
        public string[]? Especialidades { get; set; }
    }
}