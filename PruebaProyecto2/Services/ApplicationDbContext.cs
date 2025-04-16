using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using GYMISFAMILY.Models.BaseDeDatos;
using GYMISFAMILY.Models;
using Microsoft.AspNetCore.Identity;

namespace GYMISFAMILY.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string> // Hereda de IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Tablas de la base de datos personalizadas
        public DbSet<TipoMembresia> TiposMembresia { get; set; }
        public DbSet<MembresiaUsuario> MembresiasUsuarios { get; set; }
        public DbSet<Productos> Productos { get; set; }
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<DetallesVenta> DetallesVentas { get; set; }
        public DbSet<Acceso> Accesos { get; set; }
        public DbSet<Gasto> Gastos { get; set; }
        public DbSet<Caja> Caja { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);  // Llamada a la configuración predeterminada de Identity

            // Configuración adicional para tablas personalizadas
            modelBuilder.Entity<ApplicationUser>(entity =>
            {
                entity.HasIndex(u => u.Email).IsUnique();
                entity.HasIndex(u => u.RFID).IsUnique();
            });

            modelBuilder.Entity<MembresiaUsuario>(entity =>
            {
                entity.HasOne(mu => mu.Usuario)
                      .WithMany(u => u.MembresiasUsuarios);

                entity.HasOne(mu => mu.TipoMembresia)
                      .WithMany(tm => tm.MembresiasUsuarios)
                      .HasForeignKey(mu => mu.ID_TipoMembresia);
            });

            modelBuilder.Entity<DetallesVenta>(entity =>
            {
                entity.HasOne(dv => dv.Venta)
                      .WithMany(v => v.DetallesVentas)
                      .HasForeignKey(dv => dv.ID_Venta);

                entity.HasOne(dv => dv.Producto)
                      .WithMany(p => p.DetallesVentas)
                      .HasForeignKey(dv => dv.ID_Producto);
            });

            modelBuilder.Entity<Acceso>(entity =>
            {
                entity.HasOne(a => a.Usuario)
                      .WithMany(u => u.Accesos);
            });
        }
    }


}
