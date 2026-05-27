public class UpdateUsuarioDto
{
    public string Nombre { get; set; } = string.Empty;
    public string Ape1 { get; set; } = string.Empty;
    public string? Ape2 { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? Rol { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? Telefono { get; set; }
    public string? Direccion { get; set; }
    public string? NumLicencia { get; set; }
    public string? HorarioJson { get; set; }
}