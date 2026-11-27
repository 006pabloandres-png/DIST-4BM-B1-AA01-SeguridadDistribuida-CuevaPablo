using System.ComponentModel.DataAnnotations;

namespace Auth.Api.DTOs
{
    public class RegistroRequestDTO
    {
        [Required, MaxLength(150)]
        public string NombreCompleto { get; set; } = string.Empty;

        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string Password { get; set; } = string.Empty;

        [Required, Compare(nameof(Password), ErrorMessage = "Las contraseñas no coinciden.")]
        public string ConfirmarPassword { get; set; } = string.Empty;
    }

    public class LoginRequestDTO
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }

    public class GoogleLoginRequestDTO
    {
        [Required]
        public string IdToken { get; set; } = string.Empty;
    }

    public class RecuperarPasswordRequestDTO
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;
    }

    public class RestablecerPasswordRequestDTO
    {
        [Required, EmailAddress]
        public string Correo { get; set; } = string.Empty;

        [Required]
        public string Token { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NuevaPassword { get; set; } = string.Empty;
    }

    public class CambiarPasswordRequestDTO
    {
        [Required]
        public string PasswordActual { get; set; } = string.Empty;

        [Required, MinLength(8)]
        public string NuevaPassword { get; set; } = string.Empty;
    }

    public class CambiarRolDTO
    {
        [Required]
        public string NuevoRol { get; set; } = string.Empty; // Cliente | Administrador
    }

    public class CambiarEstadoCuentaDTO
    {
        public bool Habilitada { get; set; }
    }
}