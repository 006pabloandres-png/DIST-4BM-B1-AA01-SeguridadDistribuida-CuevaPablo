using Auth.Api.Models;
using Microsoft.AspNetCore.Identity;

namespace Auth.Api.Data
{
    public static class DbSeeder
    {
        public static async Task InicializarRolesYAdminAsync(IServiceProvider services, IConfiguration config)
        {
            var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            string[] roles = { "Cliente", "Administrador" };
            foreach (var rol in roles)
            {
                if (!await roleManager.RoleExistsAsync(rol))
                    await roleManager.CreateAsync(new IdentityRole(rol));
            }

            // Credenciales leídas desde User Secrets o variables de entorno (nunca hardcodeadas)
            var adminEmail = config["AdminSeed:Email"];
            var adminPassword = config["AdminSeed:Password"];

            if (!string.IsNullOrWhiteSpace(adminEmail) && !string.IsNullOrWhiteSpace(adminPassword))
            {
                var adminExistente = await userManager.FindByEmailAsync(adminEmail);
                if (adminExistente == null)
                {
                    var admin = new ApplicationUser
                    {
                        UserName = adminEmail,
                        Email = adminEmail,
                        EmailConfirmed = true,
                        NombreCompleto = "Administrador Inicial",
                        CuentaHabilitada = true
                    };

                    var resultado = await userManager.CreateAsync(admin, adminPassword);
                    if (resultado.Succeeded)
                        await userManager.AddToRoleAsync(admin, "Administrador");
                }
            }
        }
    }
}