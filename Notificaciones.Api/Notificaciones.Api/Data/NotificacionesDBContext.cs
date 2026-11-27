using Microsoft.EntityFrameworkCore;
using Notificaciones.Api.Models;

namespace Notificaciones.Api.Data
{
    public class NotificacionesDBContext : DbContext
    {
        public NotificacionesDBContext(DbContextOptions<NotificacionesDBContext> options) : base(options) { }

        public DbSet<EnvioCorreo> EnviosCorreo { get; set; }
    }
}