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
    [Authorize] // Requiere autenticación
    public class RecursosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public RecursosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST: api/Recursos/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string nombre, [FromForm] string tipo)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { mensaje = "No se ha enviado ningún archivo" });

            // Validar tipo
            var tiposPermitidos = new[] { "PDF", "VIDEO", "ENLACE", "PLANTILLA" };
            if (!tiposPermitidos.Contains(tipo))
                return BadRequest(new { mensaje = "Tipo no válido. Debe ser PDF, VIDEO, ENLACE o PLANTILLA" });

            // Obtener usuario autenticado
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var usuarioId = Guid.Parse(userIdClaim);

            // Obtener el centro del usuario (tomamos el primero activo)
            var usuarioCentro = await _context.UsuarioCentros
                .FirstOrDefaultAsync(uc => uc.IdUsuario == usuarioId && uc.Activo);
            if (usuarioCentro == null)
                return BadRequest(new { mensaje = "Usuario no asociado a ningún centro" });

            // Ruta de almacenamiento
            var uploadsFolder = "/app/data/uploads";
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generar nombre único
            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Guardar en BD
            var recurso = new Recurso
            {
                IdRecurso = Guid.NewGuid(),
                Nombre = string.IsNullOrEmpty(nombre) ? file.FileName : nombre,
                Tipo = tipo,
                UrlAlmacenamiento = $"/uploads/{uniqueFileName}", //Ruta relativa para servir el archivo
                TamanioBytes = file.Length,
                FechaSubida = DateTime.UtcNow,
                SubidoPor = usuarioId,
                IdCentro = usuarioCentro.IdCentro
            };

            _context.Recursos.Add(recurso);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                id = recurso.IdRecurso,
                nombre = recurso.Nombre,
                url = recurso.UrlAlmacenamiento,
                tipo = recurso.Tipo,
                tamano = recurso.TamanioBytes
            });
        }

        // GET: api/Recursos
        [HttpGet]
        public async Task<ActionResult<List<Recurso>>> GetRecursos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var usuarioId = Guid.Parse(userIdClaim);

            // Obtener el centro del usuario
            var usuarioCentro = await _context.UsuarioCentros
                .FirstOrDefaultAsync(uc => uc.IdUsuario == usuarioId && uc.Activo);
            if (usuarioCentro == null)
                return new List<Recurso>();

            var recursos = await _context.Recursos
                .Where(r => r.IdCentro == usuarioCentro.IdCentro)
                .OrderByDescending(r => r.FechaSubida)
                .ToListAsync();

            return Ok(recursos);
        }

        // DELETE: api/Recursos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRecurso(Guid id)
        {
            var recurso = await _context.Recursos.FindAsync(id);
            if (recurso == null)
                return NotFound();

            // Verificar permisos: solo quien lo subió o admin
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            var usuarioId = Guid.Parse(userIdClaim);

            var esAdmin = User.IsInRole("ADMIN");
            if (recurso.SubidoPor != usuarioId && !esAdmin)
                return Forbid();

            // Determinar la ruta de almacenamiento
            var uploadsFolder = "/app/data/uploads";
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT")))
            {
                uploadsFolder = Path.Combine(_env.ContentRootPath, "uploads");
            }

            // Eliminar archivo físico
            var filePath = Path.Combine(uploadsFolder, recurso.UrlAlmacenamiento.TrimStart('/'));
            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Recursos.Remove(recurso);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Recurso eliminado" });
        }
    }
}