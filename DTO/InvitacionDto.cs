using Balance.API.Models;

namespace Balance.API.DTO
{
    public class CrearInvitacionDto
    {
        public string Email { get; set; } = string.Empty;
        public int Rol { get; set; }
        public Guid IdCentro { get; set; }
        public int DiasExpiracion { get; set; } = 7;
    }

    public class InvitacionResponseDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int IdRol { get; set; }
        public DateTime ExpiraEn { get; set; }
        public bool Usada { get; set; }
    }
}