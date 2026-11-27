using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        public bool CuentaHabilitada { get; set; } = true;

        public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;
    }
}