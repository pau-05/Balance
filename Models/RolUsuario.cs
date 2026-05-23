namespace Balance.API.Models
{
    public enum RolUsuario
    {
        ADMIN = 1,
        PSICOLOGO = 2,
        PACIENTE = 3
    }

    // Clase auxiliar para convertir entre enum y string
    public static class RolUsuarioExtensions
    {
        public static string ToStringValue(this RolUsuario rol)
        {
            return rol switch
            {
                RolUsuario.ADMIN => "ADMIN",
                RolUsuario.PSICOLOGO => "PSICOLOGO",
                RolUsuario.PACIENTE => "PACIENTE",
                _ => throw new ArgumentException("Rol no válido")
            };
        }

        public static RolUsuario FromString(string rol)
        {
            return rol.ToUpper() switch
            {
                "ADMIN" => RolUsuario.ADMIN,
                "PSICOLOGO" => RolUsuario.PSICOLOGO,
                "PACIENTE" => RolUsuario.PACIENTE,
                _ => throw new ArgumentException($"Rol '{rol}' no válido")
            };
        }

        public static bool IsValid(string rol)
        {
            return rol.ToUpper() is "ADMIN" or "PSICOLOGO" or "PACIENTE";
        }
    }
}