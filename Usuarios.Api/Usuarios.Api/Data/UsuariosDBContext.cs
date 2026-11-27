using Microsoft.EntityFrameworkCore;
using Usuarios.Api.Models;

namespace Usuarios.Api.Data
{
    public class UsuariosDBContext : DbContext
    {
        public UsuariosDBContext(DbContextOptions<UsuariosDBContext> options) : base(options) { }

        public DbSet<Perfil> Perfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Perfil>()
                .HasIndex(p => p.UsuarioId)
                .IsUnique();
        }
    }
}