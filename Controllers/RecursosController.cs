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
    [Authorize] // ← Reactivar autenticación
    public class RecursosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        // 🔥 Ruta única y consistente para toda la clase
        private readonly string _uploadsFolder = "/app/data/uploads";

        public RecursosController(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // POST: api/Recursos/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, [FromForm] string nombre, [FromForm] string tipo)
        {
            try
            {
                // 🔥 LOGS DE DEPURACIÓN
                Console.WriteLine($"=== UPLOAD DEBUG ===");
                Console.WriteLine($"File: {file?.FileName}");
                Console.WriteLine($"File length: {file?.Length}");
                Console.WriteLine($"Nombre: {nombre}");
                Console.WriteLine($"Tipo: {tipo}");
                Console.WriteLine($"User authenticated: {User.Identity?.IsAuthenticated}");

                if (file == null || file.Length == 0)
                {
                    Console.WriteLine("ERROR: Archivo nulo o vacío");
                    return BadRequest(new { mensaje = "No se ha enviado ningún archivo" });
                }

                // Validar tipo
                var tiposPermitidos = new[] { "PDF", "VIDEO", "ENLACE", "PLANTILLA" };
                if (!tiposPermitidos.Contains(tipo))
                {
                    Console.WriteLine($"ERROR: Tipo no válido: {tipo}");
                    return BadRequest(new { mensaje = "Tipo no válido. Debe ser PDF, VIDEO, ENLACE o PLANTILLA" });
                }

                // Obtener usuario autenticado
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                Console.WriteLine($"UserIdClaim: {userIdClaim}");

                if (string.IsNullOrEmpty(userIdClaim))
                {
                    Console.WriteLine("ERROR: Usuario no autenticado");
                    return Unauthorized();
                }

                var usuarioId = Guid.Parse(userIdClaim);

                // Obtener el centro del usuario
                var usuarioCentro = await _context.UsuarioCentros
                    .FirstOrDefaultAsync(uc => uc.IdUsuario == usuarioId && uc.Activo);

                if (usuarioCentro == null)
                {
                    Console.WriteLine($"ERROR: Usuario {usuarioId} no tiene centro asociado");
                    return BadRequest(new { mensaje = "Usuario no asociado a ningún centro" });
                }

                Console.WriteLine($"Centro encontrado: {usuarioCentro.IdCentro}");

                // Ruta de almacenamiento
                var uploadsFolder = "/app/data/uploads";
                Console.WriteLine($"Uploads folder: {uploadsFolder}");
                Console.WriteLine($"Folder exists: {Directory.Exists(uploadsFolder)}");

                if (!Directory.Exists(uploadsFolder))
                {
                    Console.WriteLine("Creando carpeta de uploads...");
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Generar nombre único
                var fileExtension = Path.GetExtension(file.FileName);
                var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                Console.WriteLine($"Saving to: {filePath}");

                // Guardar archivo
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                Console.WriteLine($"File saved successfully. Size: {new FileInfo(filePath).Length} bytes");

                // Guardar en BD
                var recurso = new Recurso
                {
                    IdRecurso = Guid.NewGuid(),
                    Nombre = string.IsNullOrEmpty(nombre) ? file.FileName : nombre,
                    Tipo = tipo,
                    UrlAlmacenamiento = $"/uploads/{uniqueFileName}",
                    TamanioBytes = file.Length,
                    FechaSubida = DateTime.UtcNow,
                    SubidoPor = usuarioId,
                    IdCentro = usuarioCentro.IdCentro
                };

                _context.Recursos.Add(recurso);
                await _context.SaveChangesAsync();

                Console.WriteLine($"Recurso guardado en BD con ID: {recurso.IdRecurso}");

                return Ok(new
                {
                    mensaje = "Archivo subido correctamente",
                    id = recurso.IdRecurso,
                    nombre = recurso.Nombre,
                    url = recurso.UrlAlmacenamiento,
                    tamano = recurso.TamanioBytes
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPCIÓN: {ex.Message}");
                Console.WriteLine($"STACK TRACE: {ex.StackTrace}");
                return StatusCode(500, new { mensaje = $"Error interno: {ex.Message}" });
            }
        }

        // GET: api/Recursos
        [HttpGet]
        public async Task<ActionResult<List<Recurso>>> GetRecursos()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            var usuarioId = Guid.Parse(userIdClaim);

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

            // Verificar permisos
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();
            var usuarioId = Guid.Parse(userIdClaim);

            var esAdmin = User.IsInRole("ADMIN");
            if (recurso.SubidoPor != usuarioId && !esAdmin)
                return Forbid();

            //Usar la misma ruta consistente
            var filePath = Path.Combine(_uploadsFolder, recurso.UrlAlmacenamiento.TrimStart('/'));

            if (System.IO.File.Exists(filePath))
                System.IO.File.Delete(filePath);

            _context.Recursos.Remove(recurso);
            await _context.SaveChangesAsync();

            return Ok(new { mensaje = "Recurso eliminado" });
        }

        // GET: api/Recursos/diagnostic
        [HttpGet("diagnostic")]
        [AllowAnonymous] // ← Permitir diagnóstico sin autenticación
        public IActionResult Diagnostic()
        {
            var results = new List<object>();

            var possiblePaths = new[]
            {
                "/app/data/uploads",           // Volumen de Railway
                "/app/uploads",                 // Otra posible ruta
                Path.Combine(Directory.GetCurrentDirectory(), "uploads"),
                Path.Combine(_env.ContentRootPath, "uploads")
            };

            foreach (var path in possiblePaths)
            {
                var exists = Directory.Exists(path);
                var writable = false;

                if (exists)
                {
                    try
                    {
                        var testFile = Path.Combine(path, "test.txt");
                        System.IO.File.WriteAllText(testFile, "test");
                        System.IO.File.Delete(testFile);
                        writable = true;
                    }
                    catch
                    {
                        writable = false;
                    }
                }

                results.Add(new { path, exists, writable });
            }

            // 🔥 También verificar la ruta que estamos usando actualmente
            var currentUploadsExists = Directory.Exists(_uploadsFolder);
            var currentUploadsWritable = false;
            if (currentUploadsExists)
            {
                try
                {
                    var testFile = Path.Combine(_uploadsFolder, "test_current.txt");
                    System.IO.File.WriteAllText(testFile, "test");
                    System.IO.File.Delete(testFile);
                    currentUploadsWritable = true;
                }
                catch { }
            }

            return Ok(new
            {
                environment = Environment.GetEnvironmentVariable("RAILWAY_ENVIRONMENT") ?? "local",
                railwayGitRepo = Environment.GetEnvironmentVariable("RAILWAY_GIT_REPO_NAME") ?? "no",
                contentRootPath = _env.ContentRootPath,
                currentDirectory = Directory.GetCurrentDirectory(),
                currentUploadsFolder = _uploadsFolder,
                currentUploadsExists,
                currentUploadsWritable,
                paths = results
            });
        }
    }
}