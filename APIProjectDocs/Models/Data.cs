using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace APIProjectDocs.Models
{
    public class Data
    {
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
            {
            }

            // DbSets
            public DbSet<Usuario> Usuarios { get; set; }
            public DbSet<Plataforma> Plataformas { get; set; }
            public DbSet<Proyecto> Proyectos { get; set; }
            public DbSet<Entregable> Entregables { get; set; }
            public DbSet<ComprobantePago> ComprobantesPago { get; set; }
            public DbSet<UsuarioPlataforma> UsuarioPlataformas { get; set; }
            public DbSet<UsuarioProyecto> UsuarioProyectos { get; set; }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                // Configuración de Usuario
                modelBuilder.Entity<Usuario>(entity =>
                {
                    entity.HasIndex(u => u.Email).IsUnique();
                    entity.Property(u => u.Rol).HasConversion<int>();
                });

                // Configuración de Plataforma
                modelBuilder.Entity<Plataforma>(entity =>
                {
                    entity.HasIndex(p => p.Nombre).IsUnique();
                });

                // Configuración de Proyecto
                modelBuilder.Entity<Proyecto>(entity =>
                {
                    entity.HasOne(p => p.Plataforma)
                          .WithMany(pl => pl.Proyectos)
                          .HasForeignKey(p => p.IdPlataforma)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasIndex(p => new { p.Nombre, p.IdPlataforma }).IsUnique();
                });

                // Configuración de Entregable
                modelBuilder.Entity<Entregable>(entity =>
                {
                    entity.HasOne(e => e.Proyecto)
                          .WithMany(p => p.Entregables)
                          .HasForeignKey(e => e.IdProyecto)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.Property(e => e.OrigenDatos).HasConversion<int>();

                    entity.HasIndex(e => new { e.Nombre, e.IdProyecto }).IsUnique();
                });

                // Configuración de ComprobantePago
                modelBuilder.Entity<ComprobantePago>(entity =>
                {
                    entity.HasOne(c => c.Entregable)
                          .WithMany(e => e.ComprobantesPago)
                          .HasForeignKey(c => c.IdEntregable)
                          .OnDelete(DeleteBehavior.Restrict);

                    entity.HasOne(c => c.UsuarioSubida)
                          .WithMany()
                          .HasForeignKey(c => c.IdUsuarioSubida)
                          .OnDelete(DeleteBehavior.Restrict);
                });

                // Configuración de UsuarioPlataforma (tabla intermedia)
                modelBuilder.Entity<UsuarioPlataforma>(entity =>
                {
                    entity.HasOne(up => up.Usuario)
                          .WithMany(u => u.UsuarioPlataformas)
                          .HasForeignKey(up => up.IdUsuario)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(up => up.Plataforma)
                          .WithMany(p => p.UsuarioPlataformas)
                          .HasForeignKey(up => up.IdPlataforma)
                          .OnDelete(DeleteBehavior.Cascade);

                    // Un usuario puede ser coordinador de múltiples plataformas
                    entity.HasIndex(up => new { up.IdUsuario, up.IdPlataforma }).IsUnique();
                });

                // Configuración de UsuarioProyecto (tabla intermedia)
                modelBuilder.Entity<UsuarioProyecto>(entity =>
                {
                    entity.HasOne(up => up.Usuario)
                          .WithMany(u => u.UsuarioProyectos)
                          .HasForeignKey(up => up.IdUsuario)
                          .OnDelete(DeleteBehavior.Cascade);

                    entity.HasOne(up => up.Proyecto)
                          .WithMany(p => p.UsuarioProyectos)
                          .HasForeignKey(up => up.IdProyecto)
                          .OnDelete(DeleteBehavior.Cascade);

                    // Un usuario puede ser líder de múltiples proyectos
                    entity.HasIndex(up => new { up.IdUsuario, up.IdProyecto }).IsUnique();
                });

                // Datos semilla (seed data)
                SeedData(modelBuilder);
            }

            private void SeedData(ModelBuilder modelBuilder)
            {
                // Usuario administrador por defecto
                modelBuilder.Entity<Usuario>().HasData(
                    new Usuario
                    {
                        IdUsuario = 1,
                        Nombre = "Administrador Sistema",
                        Email = "admin@empresa.com",
                        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"), // Cambiar en producción
                        Rol = RolUsuario.Direccion,
                        Activo = true,
                        FechaCreacion = DateTime.UtcNow
                    }
                );

                // Plataforma por defecto
                modelBuilder.Entity<Plataforma>().HasData(
                    new Plataforma
                    {
                        IdPlataforma = 1,
                        Nombre = "Plataforma General",
                        Descripcion = "Plataforma inicial del sistema",
                        Activa = true,
                        FechaCreacion = DateTime.UtcNow
                    }
                );
            }
        }
    }
}
