using Balance.API.Data;
using Balance.API.DTO;
using Balance.API.Models;
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

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            // Buscar psicólogo por Email y NumeroColegiado
            var psicologo = await _context.Psicologos
                .FirstOrDefaultAsync(p => p.Email == loginDto.Email && p.NumeroColegiado == loginDto.NumeroColegiado);

            if (psicologo == null)
                return Unauthorized(new { message = "Email o número de colegiado incorrectos" });

            // Generar token JWT
            var token = GenerateJwtToken(psicologo);
            return Ok(new { token, psicologoId = psicologo.Id, nombre = $"{psicologo.Nombre} {psicologo.Apellidos}" });
        }

        private string GenerateJwtToken(Psicologo psicologo)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.ASCII.GetBytes(jwtSettings["Secret"]!);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, psicologo.Id.ToString()),
                new Claim(ClaimTypes.Email, psicologo.Email),
                new Claim("NumeroColegiado", psicologo.NumeroColegiado),
                // Puedes agregar roles si los tienes
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(double.Parse(jwtSettings["ExpirationInMinutes"]!)),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}