using Balance.API.Data;
using Balance.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using Balance.API.DTO;
using Balance.API.Converters;

namespace Balance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsuariosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public UsuariosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Usuarios/{id}/centro
        [HttpGet("{id}/centro")]
        public async Task<IActionResult> GetCentroDeUsuario(Guid id)
        {
            try
            {
                var usuarioCentro = await _context.UsuarioCentros
                    .FirstOrDefaultAsync(uc => uc.IdUsuario == id && uc.Activo);

                if (usuarioCentro == null)
                {
                    return NotFound(new { mensaje = "Usuario no tiene centro asociado" });
                }

                return Ok(new { centroId = usuarioCentro.IdCentro });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/Usuarios/actual
        [HttpGet("actual")]
        public async Task<IActionResult> GetUsuarioActual()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var usuarioId = Guid.Parse(userIdClaim);
            var usuario = await _context.Usuarios.FindAsync(usuarioId);

            if (usuario == null)
                return NotFound();

            return Ok(new { usuario.Id, usuario.Nombre, usuario.Ape1, usuario.Email });
        }

        //Endpoint para obtener todos los usuarios de un centro determinado
        [HttpGet("centro/{centroId}")]
        public async Task<ActionResult<List<object>>> GetUsuariosPorCentro(Guid centroId)
        {
            var usuarios = await _context.UsuarioCentros
                .Where(uc => uc.IdCentro == centroId && uc.Activo)
                .Include(uc => uc.Usuario)
                .Include(uc => uc.Rol)
                .Select(uc => new
                {
                    uc.Usuario.Id,
                    uc.Usuario.Nombre,
                    uc.Usuario.Ape1,
                    uc.Usuario.Ape2,
                    uc.Usuario.Email,
                    uc.Usuario.FechaRegistro,
                    Rol = uc.Rol.Nombre
                })
                .ToListAsync();

            return Ok(usuarios);
        }

        //Endpoint para actualizar usuario
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUsuario(Guid id, [FromBody] UpdateUsuarioDto dto)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();

            usuario.Nombre = dto.Nombre;
            usuario.Ape1 = dto.Ape1;
            usuario.Ape2 = dto.Ape2 ?? usuario.Ape2;
            usuario.Email = dto.Email;

            if (dto.Rol == "PACIENTE")
            {
                var paciente = await _context.Pacientes.FindAsync(id);
                if (paciente != null)
                {
                    if (dto.FechaNacimiento.HasValue)
                        paciente.FechaNacimiento = dto.FechaNacimiento.Value;
                    if (!string.IsNullOrEmpty(dto.Telefono))
                        paciente.Telefono = dto.Telefono;
                    if (!string.IsNullOrEmpty(dto.Direccion))
                        paciente.Direccion = dto.Direccion;
                }
            }
            else if (dto.Rol == "PSICOLOGO")
            {
                var psicologo = await _context.Psicologos.FindAsync(id);
                if (psicologo != null)
                {
                    if (!string.IsNullOrEmpty(dto.NumLicencia))
                        psicologo.NumLicencia = dto.NumLicencia;

                    if (!string.IsNullOrEmpty(dto.HorarioJson))
                    {
                        var horarioJson = ConvertidorJsonAString.ConvertirHorarioAJson(dto.HorarioJson);
                        psicologo.Horario = JsonDocument.Parse(horarioJson);
                    }
                }
            }

            await _context.SaveChangesAsync();
            return Ok();
        }

        //Endpoint para obtener un usuario en especifico:
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUsuario(Guid id)
        {
            var usuario = await _context.Usuarios.FindAsync(id);
            if (usuario == null) return NotFound();
            return Ok(usuario);
        }

        //Obtener un psicologo
        [HttpGet("psicologo/{id}")]
        public async Task<IActionResult> GetPsicologo(Guid id)
        {
            var psicologo = await _context.Psicologos.FindAsync(id);
            if (psicologo == null) return NotFound();

            //Tranforma de Json Document a String
            var horarioTexto = ConvertidorJsonAString.ConvertirJsonAString(psicologo.Horario);
            Console.WriteLine($"HorarioTexto generado: '{horarioTexto}'");

            return Ok(new
            {
                psicologo.IdUsuario,
                psicologo.NumLicencia,
                Horario = horarioTexto
            });
        }
    }
}