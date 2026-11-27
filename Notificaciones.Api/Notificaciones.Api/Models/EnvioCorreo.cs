using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Notificaciones.Api.Models
{
    [Table("EnviosCorreo")]
    public class EnvioCorreo
    {
        [Key]
        public int EnvioId { get; set; }

        [Required]
        [MaxLength(450)]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Destinatario { get; set; } = string.Empty;

        [Required]
        [MaxLength(200)]
        public string Asunto { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TipoNotificacion { get; set; } = string.Empty; // Bienvenida, Recuperacion, CambioPerfil

        [Required]
        [MaxLength(30)]
        public string EstadoEnvio { get; set; } = string.Empty; // Enviado, Fallido

        [MaxLength(500)]
        public string? DetalleError { get; set; }

        public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
    }
}