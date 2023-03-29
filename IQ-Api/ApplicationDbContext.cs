using IQ_Api.DTOs;
using IQ_Api.Entidades;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace IQ_Api
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }



        //public DbSet<Users> Usuarios { get; set; }

        public DbSet<CredencialesUsuario> Usuarios { get; set; }
        
        public DbSet<Modulos> Modulos { get; set; }
        public DbSet<Submodulos> Submodulos { get; set; }
    }
}
