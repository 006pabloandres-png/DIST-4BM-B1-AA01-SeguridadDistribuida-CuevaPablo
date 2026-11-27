using System.ComponentModel.DataAnnotations;

namespace Notificaciones.Api.DTOs
{
    public class EnviarCorreoRequestDTO
    {
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Destinatario { get; set; } = string.Empty;

        [Required]
        public string Asunto { get; set; } = string.Empty;

        [Required]
        public string CuerpoHtml { get; set; } = string.Empty;

        [Required]
        public string TipoNotificacion { get; set; } = string.Empty;
    }
}