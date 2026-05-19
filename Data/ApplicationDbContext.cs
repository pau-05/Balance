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
    }
}