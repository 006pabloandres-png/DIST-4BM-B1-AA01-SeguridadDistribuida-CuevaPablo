using System.Net;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Auth.Api.DTOs;
using Auth.Api.Models;
using Auth.Api.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly JwtService _jwtService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            JwtService jwtService,
            IHttpClientFactory httpClientFactory,
            IConfiguration config)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _httpClientFactory = httpClientFactory;
            _config = config;
        }

        [HttpPost("registro")]
        public async Task<IActionResult> Registrar([FromBody] RegistroRequestDTO dto)
        {
            if (await _userManager.FindByEmailAsync(dto.Correo) != null)
                return Conflict(new { mensaje = "El correo electrónico ya se encuentra registrado." });

            var usuario = new ApplicationUser
            {
                UserName = dto.Correo,
                Email = dto.Correo,
                NombreCompleto = dto.NombreCompleto,
                CuentaHabilitada = true
            };

            var resultado = await _userManager.CreateAsync(usuario, dto.Password);
            if (!resultado.Succeeded)
                return BadRequest(new { mensaje = "Datos inválidos.", errores = resultado.Errors.Select(e => e.Description) });

            await _userManager.AddToRoleAsync(usuario, "Cliente");

            // Comunicación con Usuarios.Api con compensación ante falla o timeout
            var usuariosClient = _httpClientFactory.CreateClient("UsuariosClient");
            var perfilPayload = new StringContent(
                JsonSerializer.Serialize(new
                {
                    UsuarioId = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Correo = usuario.Email
                }),
                Encoding.UTF8,
                "application/json");

            try
            {
                var respPerfil = await usuariosClient.PostAsync("api/usuarios/interno", perfilPayload);
                if (!respPerfil.IsSuccessStatusCode)
                {
                    // Compensación: eliminar identidad en AuthDB para no dejar cuenta inconsistente
                    await _userManager.DeleteAsync(usuario);
                    return StatusCode(503, new { mensaje = "No se pudo crear el perfil en Usuarios.Api. Registro revertido." });
                }
            }
            catch (TaskCanceledException)
            {
                await _userManager.DeleteAsync(usuario);
                return StatusCode(408, new { mensaje = "Tiempo de espera agotado al contactar Usuarios.Api. Registro revertido." });
            }
            catch (HttpRequestException)
            {
                await _userManager.DeleteAsync(usuario);
                return StatusCode(503, new { mensaje = "Servicio Usuarios.Api no disponible. Registro revertido." });
            }

            // Notificación de bienvenida (no revierte el registro si falla el correo)
            await EnviarNotificacionSilenciosaAsync(
                usuario.Id,
                usuario.Email!,
                "Bienvenido al Sistema Comercial Distribuido",
                $"<h3>Hola {WebUtility.HtmlEncode(usuario.NombreCompleto)}</h3><p>Tu cuenta ha sido creada exitosamente con rol <b>Cliente</b>.</p>",
                "Bienvenida");

            return StatusCode(201, new
            {
                mensaje = "Usuario registrado correctamente.",
                usuarioId = usuario.Id,
                correo = usuario.Email,
                rol = "Cliente"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Correo);
            if (usuario == null)
                return Unauthorized(new { mensaje = "Credenciales inválidas." });

            if (!usuario.CuentaHabilitada)
                return StatusCode(403, new { mensaje = "La cuenta se encuentra deshabilitada por un administrador." });

            var check = await _signInManager.CheckPasswordSignInAsync(usuario, dto.Password, lockoutOnFailure: true);

            if (check.IsLockedOut)
                return StatusCode(423, new { mensaje = "Cuenta bloqueada temporalmente por múltiples intentos fallidos." });

            if (!check.Succeeded)
                return Unauthorized(new { mensaje = "Credenciales inválidas." });

            var roles = await _userManager.GetRolesAsync(usuario);
            var (token, expiracion) = _jwtService.GenerarToken(usuario, roles);

            return Ok(new
            {
                token,
                expiracion,
                usuarioId = usuario.Id,
                correo = usuario.Email,
                roles
            });
        }

        [HttpPost("google")]
        public async Task<IActionResult> LoginConGoogle([FromBody] GoogleLoginRequestDTO dto)
        {
            GoogleJsonWebSignature.Payload payload;
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _config["Google:ClientId"]! }
                };
                payload = await GoogleJsonWebSignature.ValidateAsync(dto.IdToken, settings);
            }
            catch (Exception)
            {
                return Unauthorized(new { mensaje = "Token de identidad de Google inválido o expirado." });
            }

            if (!payload.EmailVerified)
                return Unauthorized(new { mensaje = "El correo de la cuenta de Google no está verificado." });

            // Relacionar por el identificador estable 'sub' del proveedor
            var usuario = await _userManager.FindByLoginAsync("Google", payload.Subject);

            if (usuario == null)
            {
                // Evitar vinculación automática insegura si ya existe una cuenta local con ese correo
                var usuarioLocalExistente = await _userManager.FindByEmailAsync(payload.Email);
                if (usuarioLocalExistente != null)
                {
                    return Conflict(new
                    {
                        mensaje = "Ya existe una cuenta local registrada con este correo. Inicie sesión con su contraseña local."
                    });
                }

                usuario = new ApplicationUser
                {
                    UserName = payload.Email,
                    Email = payload.Email,
                    EmailConfirmed = true,
                    NombreCompleto = payload.Name ?? payload.Email,
                    CuentaHabilitada = true
                };

                var createResult = await _userManager.CreateAsync(usuario);
                if (!createResult.Succeeded)
                    return BadRequest(new { mensaje = "No se pudo registrar el usuario externo." });

                await _userManager.AddToRoleAsync(usuario, "Cliente");
                await _userManager.AddLoginAsync(usuario, new UserLoginInfo("Google", payload.Subject, "Google"));

                // Crear perfil en Usuarios.Api con compensación
                var usuariosClient = _httpClientFactory.CreateClient("UsuariosClient");
                var perfilPayload = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        UsuarioId = usuario.Id,
                        NombreCompleto = usuario.NombreCompleto,
                        Correo = usuario.Email
                    }),
                    Encoding.UTF8,
                    "application/json");

                try
                {
                    var resp = await usuariosClient.PostAsync("api/usuarios/interno", perfilPayload);
                    if (!resp.IsSuccessStatusCode)
                    {
                        await _userManager.DeleteAsync(usuario);
                        return StatusCode(503, new { mensaje = "Error al crear perfil de usuario externo." });
                    }
                }
                catch (Exception)
                {
                    await _userManager.DeleteAsync(usuario);
                    return StatusCode(503, new { mensaje = "Servicio Usuarios.Api no disponible." });
                }
            }

            if (!usuario.CuentaHabilitada)
                return StatusCode(403, new { mensaje = "La cuenta se encuentra deshabilitada." });

            var roles = await _userManager.GetRolesAsync(usuario);
            var (token, expiracion) = _jwtService.GenerarToken(usuario, roles);

            return Ok(new
            {
                token,
                expiracion,
                usuarioId = usuario.Id,
                correo = usuario.Email,
                roles,
                proveedor = "Google"
            });
        }

        [HttpPost("recuperar-password")]
        public async Task<IActionResult> SolicitarRecuperacion([FromBody] RecuperarPasswordRequestDTO dto)
        {
            // Respuesta genérica obligatoria para no revelar si el correo existe
            var respuestaGenerica = Ok(new
            {
                mensaje = "Si el correo está registrado en el sistema, recibirás un enlace de restablecimiento en tu bandeja."
            });

            var usuario = await _userManager.FindByEmailAsync(dto.Correo);
            if (usuario == null || !usuario.CuentaHabilitada)
                return respuestaGenerica;

            var tokenReset = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var tokenCodificado = WebUtility.UrlEncode(tokenReset);
            var correoCodificado = WebUtility.UrlEncode(usuario.Email);
            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var enlace = $"{baseUrl}/reset-password.html?email={correoCodificado}&token={tokenCodificado}";

            var html = $@"
                <h3>Recuperación de Contraseña</h3>
                <p>Hola {WebUtility.HtmlEncode(usuario.NombreCompleto)},</p>
                <p>Haz clic en el siguiente enlace (válido por 15 minutos y de un solo uso) para restablecer tu contraseña:</p>
                <p><a href=""{enlace}"">Restablecer mi contraseña</a></p>
                <p>Si no realizaste esta solicitud, ignora este mensaje.</p>";

            // Si Notificaciones.Api falla o hace timeout, no se expone el token en la respuesta
            await EnviarNotificacionSilenciosaAsync(
                usuario.Id,
                usuario.Email!,
                "Restablecimiento de contraseña",
                html,
                "Recuperacion");

            return respuestaGenerica;
        }

        [HttpPost("restablecer-password")]
        public async Task<IActionResult> RestablecerPassword([FromBody] RestablecerPasswordRequestDTO dto)
        {
            var usuario = await _userManager.FindByEmailAsync(dto.Correo);
            if (usuario == null)
                return BadRequest(new { mensaje = "Solicitud de restablecimiento inválida o token expirado." });

            // ResetPasswordAsync valida firma/expiración y actualiza el SecurityStamp invalidando el token (un solo uso)
            var resultado = await _userManager.ResetPasswordAsync(usuario, dto.Token, dto.NuevaPassword);
            if (!resultado.Succeeded)
                return BadRequest(new { mensaje = "Token inválido, vencido o ya utilizado.", errores = resultado.Errors.Select(e => e.Description) });

            return Ok(new { mensaje = "Contraseña restablecida exitosamente. Ya puedes iniciar sesión." });
        }

        [Authorize]
        [HttpPost("cambiar-password")]
        public async Task<IActionResult> CambiarPassword([FromBody] CambiarPasswordRequestDTO dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var usuario = await _userManager.FindByIdAsync(userId!);
            if (usuario == null) return Unauthorized();

            var resultado = await _userManager.ChangePasswordAsync(usuario, dto.PasswordActual, dto.NuevaPassword);
            if (!resultado.Succeeded)
                return BadRequest(new { mensaje = "No se pudo cambiar la contraseña. Verifica tu contraseña actual.", errores = resultado.Errors.Select(e => e.Description) });

            return Ok(new { mensaje = "Contraseña actualizada correctamente." });
        }

        private async Task EnviarNotificacionSilenciosaAsync(string usuarioId, string destinatario, string asunto, string cuerpoHtml, string tipo)
        {
            try
            {
                var client = _httpClientFactory.CreateClient("NotificacionesClient");
                var content = new StringContent(
                    JsonSerializer.Serialize(new
                    {
                        UsuarioId = usuarioId,
                        Destinatario = destinatario,
                        Asunto = asunto,
                        CuerpoHtml = cuerpoHtml,
                        TipoNotificacion = tipo
                    }),
                    Encoding.UTF8,
                    "application/json");

                await client.PostAsync("api/notificaciones/enviar", content);
            }
            catch (Exception)
            {
                // Captura razonada: si Notificaciones.Api está caída o excede Timeout, no se filtra información ni tokens
            }
        }
    }
}