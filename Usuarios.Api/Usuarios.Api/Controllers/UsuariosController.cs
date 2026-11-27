using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Usuarios.Api.Data;
using Usuarios.Api.DTOs;
using Usuarios.Api.Models;

namespace Usuarios.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UsuariosDBContext _context;
        private readonly IConfiguration _config;

        public UsuariosController(UsuariosDBContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // Endpoint interno invocado por Auth.Api durante el registro
        [HttpPost("interno")]
        public async Task<IActionResult> CrearPerfilInterno([FromBody] CrearPerfilInternoDTO dto)
        {
           
            var claveEsperada = _config["InternalSecurity:ApiKey"];
            if (!Request.Headers.TryGetValue("X-Internal-Api-Key", out var clave) || clave != claveEsperada)
                return Unauthorized(new { mensaje = "Llamada interna no autorizada." });

            if (await _context.Perfiles.AnyAsync(p => p.UsuarioId == dto.UsuarioId))
                return Conflict(new { mensaje = "El perfil ya existe para este UsuarioId." });

            var perfil = new Perfil
            {
                UsuarioId = dto.UsuarioId,
                NombreCompleto = dto.NombreCompleto,
                Correo = dto.Correo,
                FechaCreacion = DateTime.UtcNow
            };

            _context.Perfiles.Add(perfil);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(ObtenerPerfilPorUsuarioId), new { usuarioId = perfil.UsuarioId }, perfil);
        }

        // Consultar perfil propio mediante JWT
        [Authorize]
        [HttpGet("mi-perfil")]
        public async Task<IActionResult> ObtenerMiPerfil()
        {
            var usuarioIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdToken))
                return Unauthorized();

            var perfil = await _context.Perfiles.FirstOrDefaultAsync(p => p.UsuarioId == usuarioIdToken);
            if (perfil == null)
                return NotFound(new { mensaje = "Perfil no encontrado." });

            return Ok(perfil);
        }

        // Editar solo el perfil propio
        [Authorize]
        [HttpPut("mi-perfil")]
        public async Task<IActionResult> ActualizarMiPerfil([FromBody] ActualizarPerfilDTO dto)
        {
            var usuarioIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(usuarioIdToken))
                return Unauthorized();

            var perfil = await _context.Perfiles.FirstOrDefaultAsync(p => p.UsuarioId == usuarioIdToken);
            if (perfil == null)
                return NotFound(new { mensaje = "Perfil no encontrado." });

            perfil.NombreCompleto = dto.NombreCompleto;
            perfil.Telefono = dto.Telefono;
            perfil.Ciudad = dto.Ciudad;
            perfil.Direccion = dto.Direccion;
            perfil.FechaActualizacion = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return Ok(new { mensaje = "Perfil actualizado correctamente.", perfil });
        }

        // Consultar perfil por ID: solo el propietario o un Administrador
        [Authorize]
        [HttpGet("{usuarioId}")]
        public async Task<IActionResult> ObtenerPerfilPorUsuarioId(string usuarioId)
        {
            var usuarioIdToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var esAdmin = User.IsInRole("Administrador");

            if (usuarioIdToken != usuarioId && !esAdmin)
                return Forbid(); // Devuelve HTTP 403 Forbidden si intenta ver perfil ajeno

            var perfil = await _context.Perfiles.FirstOrDefaultAsync(p => p.UsuarioId == usuarioId);
            if (perfil == null)
                return NotFound(new { mensaje = "Perfil no encontrado." });

            return Ok(perfil);
        }

        // Listar todos los perfiles: exclusivo Administrador
        [Authorize(Roles = "Administrador")]
        [HttpGet]
        public async Task<IActionResult> ListarTodosLosPerfiles()
        {
            var perfiles = await _context.Perfiles.ToListAsync();
            return Ok(perfiles);
        }
    }
}