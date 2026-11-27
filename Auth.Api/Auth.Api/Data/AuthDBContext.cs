using Auth.Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Data
{
    public class AuthDBContext : IdentityDbContext<ApplicationUser>
    {
        public AuthDBContext(DbContextOptions<AuthDBContext> options) : base(options) { }
    }
}