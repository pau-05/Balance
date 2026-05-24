using Balance.API.Data;
using Balance.API.DTO;
using Balance.API.Models;
using Balance.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.Security.Claims;

namespace Balance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "ADMIN")] // Solo administradores pueden crear invitaciones
    public class InvitacionesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly Random _random = new Random();

        public InvitacionesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private string GenerarCodigoUnico()
        {
            return _random.Next(100000, 999999).ToString();
        }

        [HttpPost("crear")]
        [Authorize(Roles = "ADMIN")]
        public async Task<ActionResult<InvitacionResponseDto>> CrearInvitacion(CrearInvitacionDto dto)
        {
            try
            {
                // dto.Rol ya es un int (1,2,3)
                int idRol = dto.Rol;

                // Validar que el rol existe
                var rol = await _context.Roles.FindAsync(idRol);
                if (rol == null)
                    return BadRequest(new { mensaje = $"El rol con ID {idRol} no existe" });

                // Obtener el centro del admin autenticado
                var adminIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(adminIdClaim))
                    return Unauthorized();

                var adminId = Guid.Parse(adminIdClaim);

                var adminCentro = await _context.UsuarioCentros
                    .FirstOrDefaultAsync(uc => uc.IdUsuario == adminId && uc.Activo);

                if (adminCentro == null)
                    return BadRequest(new { mensaje = "Admin no tiene centro asociado" });

                // Generar código único de 6 dígitos
                string codigo;
                var random = new Random();
                do
                {
                    codigo = random.Next(100000, 999999).ToString();
                } while (await _context.Invitaciones.AnyAsync(i => i.Codigo == codigo && i.UsadoEn == null));

                // Crear invitación
                var invitacion = new Invitacion
                {
                    Id = Guid.NewGuid(),
                    Email = dto.Email,
                    Codigo = codigo,
                    IdRol = idRol,
                    IdCentro = adminCentro.IdCentro,
                    CreadoPor = adminId,
                    CreadoEn = DateTime.UtcNow,
                    ExpiraEn = DateTime.UtcNow.AddDays(dto.DiasExpiracion),
                    Activo = true
                };

                _context.Invitaciones.Add(invitacion);
                await _context.SaveChangesAsync();

                return Ok(new InvitacionResponseDto
                {
                    Id = invitacion.Id,
                    Email = invitacion.Email,
                    Codigo = invitacion.Codigo,
                    IdRol = invitacion.IdRol,
                    ExpiraEn = invitacion.ExpiraEn
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
            }
        }

        [HttpGet("listar")]
        public async Task<ActionResult<List<InvitacionResponseDto>>> ListarInvitaciones([FromQuery] bool soloActivas = true)
        {
            var query = _context.Invitaciones.AsQueryable();

            if (soloActivas)
                query = query.Where(i => i.UsadoEn == null && i.ExpiraEn > DateTime.UtcNow && i.Activo);

            var invitaciones = await query
                .OrderByDescending(i => i.CreadoEn)
                .Select(i => new InvitacionResponseDto
                {
                    Id = i.Id,
                    Email = i.Email,
                    Codigo = i.Codigo,
                    IdRol = i.IdRol,
                    ExpiraEn = i.ExpiraEn,
                    Usada = i.UsadoEn != null
                })
                .ToListAsync();

            return Ok(invitaciones);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RevocarInvitacion(Guid id)
        {
            var invitacion = await _context.Invitaciones.FindAsync(id);
            if (invitacion == null)
                return NotFound();

            invitacion.Activo = false;
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Invitación revocada" });
        }

        [HttpGet("roles")]
        public async Task<ActionResult<List<RolDto>>> ObtenerRoles()
        {
            var roles = await _context.Roles
                .Select(r => new RolDto { Id = r.IdRol, Nombre = r.Nombre })
                .ToListAsync();
            return Ok(roles);
        }
    }
}