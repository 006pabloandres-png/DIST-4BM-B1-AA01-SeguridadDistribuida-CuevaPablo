using System.ComponentModel.DataAnnotations;

namespace Usuarios.Api.DTOs
{
    public class CrearPerfilInternoDTO
    {
        [Required]
        public string UsuarioId { get; set; } = string.Empty;

        [Required, MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, EmailAddress, MaxLength(150)]
        public string Correo { get; set; } = string.Empty;
    }

    public class ActualizarPerfilDTO
    {
        [Required, MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [MaxLength(250)]
        public string? Direccion { get; set; }
    }
}