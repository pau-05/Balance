using Balance.API.Data;
using Balance.API.DTO;
using Balance.API.Models;
using Balance.API.Services;  
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Balance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ==================== VERIFICAR CÓDIGO ====================
        [HttpPost("verificar-codigo")]
        public async Task<ActionResult<CodigoValidoResponseDto>> VerificarCodigo(VerificarCodigoDto dto)
        {
            var invitacion = await _context.Invitaciones
                .Include(i => i.Rol)
                .FirstOrDefaultAsync(i => i.Codigo == dto.Codigo
                                          && i.UsadoEn == null
                                          && i.ExpiraEn > DateTime.UtcNow
                                          && i.Activo);

            if (invitacion == null)
                return BadRequest(new { mensaje = "Código inválido o expirado" });

            return Ok(new CodigoValidoResponseDto
            {
                Valido = true,
                Email = invitacion.Email,
                Rol = invitacion.Rol.Nombre
            });
        }

        // ==================== PRIMER LOGIN (REGISTRO) ====================
        [HttpPost("primer-login")]
        public async Task<IActionResult> PrimerLogin(PrimerLoginDto dto)
        {
            // 1. Buscar y validar la invitación
            var invitacion = await _context.Invitaciones
                .FirstOrDefaultAsync(i => i.Codigo == dto.Codigo
                                          && i.Email == dto.Email
                                          && i.UsadoEn == null
                                          && i.ExpiraEn > DateTime.UtcNow
                                          && i.Activo);

            if (invitacion == null)
                return BadRequest(new { mensaje = "Código inválido o expirado. Solicita una nueva invitación." });

            // 2. Verificar que el email no esté ya registrado
            var existeUsuario = await _context.Usuarios.AnyAsync(u => u.Email == dto.Email);
            if (existeUsuario)
                return BadRequest(new { mensaje = "Este email ya está registrado. Inicia sesión normalmente." });

            // 3. Iniciar transacción
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 4. Crear usuario base
                var usuario = new Usuario
                {
                    Id = Guid.NewGuid(),
                    Nombre = dto.Nombre,
                    Ape1 = dto.Ape1,
                    Ape2 = dto.Ape2,
                    Email = dto.Email,
                    PasswordHash = ScramHasher.HashPassword(dto.Password),  // ← SCRAM
                    FechaRegistro = DateTime.UtcNow
                };
                _context.Usuarios.Add(usuario);
                await _context.SaveChangesAsync();

                // 5. Crear relación usuario-centro con el rol de la invitación
                var usuarioCentro = new UsuarioCentro
                {
                    IdUsuario = usuario.Id,
                    IdCentro = invitacion.IdCentro,
                    IdRol = invitacion.IdRol,  // ← Usar el ID del rol
                    Activo = true,
                    FechaAsignacion = DateTime.UtcNow
                };
                _context.UsuarioCentros.Add(usuarioCentro);
                _context.UsuarioCentros.Add(usuarioCentro);
                await _context.SaveChangesAsync();

                // 6. Crear datos específicos según el rol
                if (invitacion.Rol.Nombre == "PACIENTE")
                {
                    var pacienteDatos = new Paciente
                    {
                        IdUsuario = usuario.Id,
                        FechaNacimiento = dto.FechaNacimiento ?? DateTime.UtcNow.AddYears(-18),
                        Telefono = dto.Telefono,
                        Direccion = dto.Direccion
                    };
                    _context.Pacientes.Add(pacienteDatos);
                }
                else if (invitacion.Rol.Nombre == "PSICOLOGO")
                {
                    var psicologoDatos = new Psicologo
                    {
                        IdUsuario = usuario.Id,
                        NumLicencia = dto.NumLicencia ?? "PENDIENTE",
                        Especialidades = dto.Especialidades ?? Array.Empty<string>()
                    };
                    _context.Psicologos.Add(psicologoDatos);
                }

                await _context.SaveChangesAsync();

                // 7. Marcar invitación como usada
                invitacion.UsadoEn = DateTime.UtcNow;
                invitacion.Activo = false;
                await _context.SaveChangesAsync();

                // 8. Confirmar transacción
                await transaction.CommitAsync();

                // 9. Generar token JWT
                var token = GenerateJwtToken(usuario);

                return Ok(new
                {
                    token,
                    usuarioId = usuario.Id,
                    nombre = $"{usuario.Nombre} {usuario.Ape1}",
                    rol = invitacion.Rol,
                    mensaje = "Registro completado exitosamente"
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
            }
        }

        // ==================== LOGIN NORMAL ====================
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                Console.WriteLine($"=== LOGIN ATTEMPT ===");
                Console.WriteLine($"Email: {dto?.Email}");
                Console.WriteLine($"Password: {dto?.Password}");

                if (dto == null || string.IsNullOrEmpty(dto.Email) || string.IsNullOrEmpty(dto.Password))
                {
                    Console.WriteLine("ERROR: Datos inválidos");
                    return BadRequest(new { mensaje = "Email y contraseña son requeridos" });
                }

                var usuario = await _context.Usuarios
                    .FirstOrDefaultAsync(u => u.Email == dto.Email);

                if (usuario == null)
                {
                    Console.WriteLine($"Usuario NO encontrado: {dto.Email}");
                    return Unauthorized(new { mensaje = "Credenciales incorrectas" });
                }

                Console.WriteLine($"Usuario encontrado: {usuario.Email}");
                Console.WriteLine($"Hash almacenado: {usuario.PasswordHash}");

                // Verificar contraseña
                bool passwordValida = ScramHasher.VerifyPassword(dto.Password, usuario.PasswordHash);

                Console.WriteLine($"Verificación de contraseña: {(passwordValida ? "VÁLIDA" : "INVÁLIDA")}");

                if (!passwordValida)
                {
                    return Unauthorized(new { mensaje = "Credenciales incorrectas" });
                }

                //if (!usuario.Activo)
                //{
                //  return Unauthorized(new { mensaje = "Cuenta desactivada. Contacta con el administrador." });
                //}

                // Obtener roles del usuario
                var roles = await _context.UsuarioCentros
                    .Where(uc => uc.IdUsuario == usuario.Id && uc.Activo)
                    .Select(uc => uc.Rol.Nombre)
                    .ToListAsync();

                Console.WriteLine($"Roles encontrados: {string.Join(", ", roles)}");

                var token = GenerateJwtToken(usuario, roles);

                return Ok(new { token, usuarioId = usuario.Id, nombre = $"{usuario.Nombre} {usuario.Ape1}", roles });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR EN LOGIN: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
            }
        }

        // ==================== GENERAR TOKEN ====================
        private string GenerateJwtToken(Usuario usuario, List<string>? roles = null)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Email, usuario.Email),
                new Claim(ClaimTypes.Name, $"{usuario.Nombre} {usuario.Ape1}")
            };

            if (roles != null)
            {
                foreach (var rol in roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, rol));
                }
            }

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(24),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }

    public class LoginDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}