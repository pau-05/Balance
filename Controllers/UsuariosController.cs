using Balance.API.Data;
using Balance.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

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
    }
}