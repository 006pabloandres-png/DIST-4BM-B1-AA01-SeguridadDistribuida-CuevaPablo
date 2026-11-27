using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Usuarios.Api.Models
{
    [Table("Perfiles")]
    public class Perfil
    {
        [Key]
        public int PerfilId { get; set; }

        [Required]
        [MaxLength(450)]
        public string UsuarioId { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required]
        [MaxLength(150)]
        public string Correo { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefono { get; set; }

        [MaxLength(100)]
        public string? Ciudad { get; set; }

        [MaxLength(250)]
        public string? Direccion { get; set; }

        public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
        public DateTime? FechaActualizacion { get; set; }
    }
}