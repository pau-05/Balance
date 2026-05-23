using Balance.API.Models;
using Microsoft.EntityFrameworkCore;

namespace Balance.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Psicologo> Psicologos { get; set; }
        public DbSet<Administrador> Administradores { get; set; }
        public DbSet<Centro> Centros { get; set; }
        public DbSet<Invitacion> Invitaciones { get; set; }
        public DbSet<Paciente> Pacientes { get; set; }
        public DbSet<Rol> Roles { get; set; }
        //public DbSet<RolUsuario> RolUsuarios { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<UsuarioCentro> UsuarioCentros { get; set; }
        public DbSet<Recurso> Recursos { get; set; }
    }
}