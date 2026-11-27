using Auth.Api.DTOs;
using Auth.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Administrador")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet("usuarios")]
        public async Task<IActionResult> ListarUsuarios()
        {
            var usuarios = await _userManager.Users.ToListAsync();
            var lista = new List<object>();

            foreach (var u in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(u);
                lista.Add(new
                {
                    u.Id,
                    u.NombreCompleto,
                    u.Email,
                    u.CuentaHabilitada,
                    u.FechaRegistro,
                    BloqueadoHasta = u.LockoutEnd,
                    Roles = roles
                });
            }

            return Ok(lista);
        }

        [HttpPut("usuarios/{id}/rol")]
        public async Task<IActionResult> CambiarRol(string id, [FromBody] CambiarRolDTO dto)
        {
            if (dto.NuevoRol != "Cliente" && dto.NuevoRol != "Administrador")
                return BadRequest(new { mensaje = "Rol no válido. Use 'Cliente' o 'Administrador'." });

            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            var rolesActuales = await _userManager.GetRolesAsync(usuario);

            // Protección del último administrador activo
            if (rolesActuales.Contains("Administrador") && dto.NuevoRol != "Administrador")
            {
                var admins = await _userManager.GetUsersInRoleAsync("Administrador");
                var adminsActivos = admins.Count(a => a.CuentaHabilitada);
                if (adminsActivos <= 1)
                    return BadRequest(new { mensaje = "Operación denegada: no se puede degradar al último administrador activo." });
            }

            await _userManager.RemoveFromRolesAsync(usuario, rolesActuales);
            await _userManager.AddToRoleAsync(usuario, dto.NuevoRol);

            return Ok(new { mensaje = $"Rol actualizado a '{dto.NuevoRol}' correctamente." });
        }

        [HttpPut("usuarios/{id}/estado")]
        public async Task<IActionResult> CambiarEstadoCuenta(string id, [FromBody] CambiarEstadoCuentaDTO dto)
        {
            var usuario = await _userManager.FindByIdAsync(id);
            if (usuario == null)
                return NotFound(new { mensaje = "Usuario no encontrado." });

            // Protección del último administrador activo
            if (!dto.Habilitada && await _userManager.IsInRoleAsync(usuario, "Administrador"))
            {
                var admins = await _userManager.GetUsersInRoleAsync("Administrador");
                var adminsActivos = admins.Count(a => a.CuentaHabilitada);
                if (adminsActivos <= 1)
                    return BadRequest(new { mensaje = "Operación denegada: no se puede deshabilitar al último administrador activo." });
            }

            usuario.CuentaHabilitada = dto.Habilitada;
            await _userManager.UpdateAsync(usuario);

            return Ok(new
            {
                mensaje = dto.Habilitada ? "Cuenta habilitada." : "Cuenta deshabilitada.",
                usuarioId = usuario.Id,
                cuentaHabilitada = usuario.CuentaHabilitada
            });
        }
    }
}