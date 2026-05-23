using Balance.API.Data;
using Balance.API.DTO;
using Balance.API.Models;
using Balance.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
        public async Task<ActionResult<InvitacionResponseDto>> CrearInvitacion(CrearInvitacionDto dto,
            [FromServices] IEmailService emailService)
        {
            // Verificar que el centro existe
            var centro = await _context.Centros.FindAsync(dto.IdCentro);
            if (centro == null)
                return BadRequest("El centro no existe");

            // Validar que el rol existe en la tabla roles
            var rol = await _context.Roles.FindAsync((int)dto.Rol);
            if (rol == null)
                return BadRequest("El rol especificado no existe");

            // Obtener el ID del administrador que crea (del token)
            var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (adminId == null)
                return Unauthorized();

            // Generar código único (asegurar que no existe ya)
            string codigo;
            do
            {
                codigo = GenerarCodigoUnico();
            } while (await _context.Invitaciones.AnyAsync(i => i.Codigo == codigo && i.UsadoEn == null));

            var invitacion = new Invitacion
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Codigo = codigo,
                IdRol = (int)dto.Rol,
                IdCentro = dto.IdCentro,
                CreadoPor = Guid.Parse(adminId),
                ExpiraEn = DateTime.UtcNow.AddDays(dto.DiasExpiracion),
                Activo = true
            };

            _context.Invitaciones.Add(invitacion);
            await _context.SaveChangesAsync();

            // TODO: Enviar email con el código
            //await _emailService.EnviarInvitacion(invitacion.Email, invitacion.Codigo, centro.Nombre);

            return Ok(new InvitacionResponseDto
            {
                Id = invitacion.Id,
                Email = invitacion.Email,
                Codigo = invitacion.Codigo,
                Rol = invitacion.Rol.Nombre,
                ExpiraEn = invitacion.ExpiraEn,
                Usada = false
            });
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
                    Rol = i.Rol.Nombre,
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