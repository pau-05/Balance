using Balance.API.Data;
using Balance.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Balance.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PsicologosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PsicologosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Psicologos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Psicologo>>> GetPsicologos()
        {
            return await _context.Psicologos.ToListAsync();
        }

        // GET: api/Psicologos/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Psicologo>> GetPsicologo(Guid id)
        {
            var psicologo = await _context.Psicologos.FindAsync(id);

            if (psicologo == null)
            {
                return NotFound();
            }

            return psicologo;
        }

        // POST: api/Psicologos
        [HttpPost]
        public async Task<ActionResult<Psicologo>> PostPsicologo(Psicologo psicologo)
        {
            psicologo.IdUsuario = Guid.NewGuid();
            _context.Psicologos.Add(psicologo);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetPsicologo), new { id = psicologo.IdUsuario }, psicologo);
        }

        // PUT: api/Psicologos/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPsicologo(Guid id, Psicologo psicologo)
        {
            if (id != psicologo.IdUsuario)
            {
                return BadRequest();
            }

            _context.Entry(psicologo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PsicologoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Psicologos/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePsicologo(Guid id)
        {
            var psicologo = await _context.Psicologos.FindAsync(id);
            if (psicologo == null)
            {
                return NotFound();
            }

            _context.Psicologos.Remove(psicologo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PsicologoExists(Guid id)
        {
            return _context.Psicologos.Any(e => e.IdUsuario == id);
        }
    }
}