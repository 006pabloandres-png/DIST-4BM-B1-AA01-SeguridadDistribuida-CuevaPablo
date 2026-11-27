using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Notificaciones.Api.Data;
using Notificaciones.Api.DTOs;
using Notificaciones.Api.Models;

namespace Notificaciones.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NotificacionesController : ControllerBase
    {
        private readonly NotificacionesDBContext _context;
        private readonly IConfiguration _config;

        public NotificacionesController(NotificacionesDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        private bool EsLlamadaInternaAutorizada()
        {
            var claveEsperada = _config["InternalSecurity:ApiKey"];
            if (string.IsNullOrWhiteSpace(claveEsperada)) return false;

            return Request.Headers.TryGetValue("X-Internal-Api-Key", out var claveRecibida)
                   && claveRecibida == claveEsperada;
        }

        [HttpPost("enviar")]
        public async Task<IActionResult> EnviarCorreo([FromBody] EnviarCorreoRequestDTO dto)
        {
            if (!EsLlamadaInternaAutorizada())
                return Unauthorized(new { mensaje = "Acceso denegado. Credencial interna inválida o ausente." });

            var registro = new EnvioCorreo
            {
                UsuarioId = dto.UsuarioId,
                Destinatario = dto.Destinatario,
                Asunto = dto.Asunto,
                TipoNotificacion = dto.TipoNotificacion,
                FechaEnvio = DateTime.UtcNow
            };

            try
            {
                var host = _config["Smtp:Host"]!;
                var port = int.Parse(_config["Smtp:Port"]!);
                var user = _config["Smtp:User"]!;
                var pass = _config["Smtp:Password"]!;
                var fromName = _config["Smtp:FromName"] ?? "Sistema Comercial Distribuido";

                using var client = new SmtpClient(host, port)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(user, pass),
                    Timeout = 10000
                };

                using var mail = new MailMessage
                {
                    From = new MailAddress(user, fromName),
                    Subject = dto.Asunto,
                    Body = dto.CuerpoHtml,
                    IsBodyHtml = true
                };
                mail.To.Add(dto.Destinatario);

                await client.SendMailAsync(mail);

                registro.EstadoEnvio = "Enviado";
                _context.EnviosCorreo.Add(registro);
                await _context.SaveChangesAsync();

                return Ok(new { mensaje = "Notificación enviada correctamente.", envioId = registro.EnvioId });
            }
            catch (Exception ex)
            {
                // Se registra el tipo de fallo sin almacenar jamás el cuerpo/token del correo
                registro.EstadoEnvio = "Fallido";
                registro.DetalleError = $"Error SMTP ({ex.GetType().Name})";
                _context.EnviosCorreo.Add(registro);
                await _context.SaveChangesAsync();

                return StatusCode(502, new { mensaje = "No se pudo completar el envío de correo en el proveedor SMTP." });
            }
        }

        [HttpGet("historial")]
        public async Task<IActionResult> ObtenerHistorial()
        {
            if (!EsLlamadaInternaAutorizada())
                return Unauthorized(new { mensaje = "Acceso denegado." });

            var historial = await _context.EnviosCorreo
                .OrderByDescending(e => e.FechaEnvio)
                .ToListAsync();

            return Ok(historial);
        }
    }
}