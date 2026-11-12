using Microsoft.EntityFrameworkCore;
using Inventario.Api.Models;

namespace Inventario.Api.Data;

public class InventarioDbContext : DbContext
{
    public InventarioDbContext(DbContextOptions<InventarioDbContext> options) : base(options)
    {
    }

    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Existencia> Existencias => Set<Existencia>();
    public DbSet<MovimientoInventario> Movimientos => Set<MovimientoInventario>();
    public DbSet<ReservaStock> Reservas => Set<ReservaStock>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Claves primarias
        modelBuilder.Entity<Bodega>().HasKey(b => b.IdBodega);
        modelBuilder.Entity<Existencia>().HasKey(e => e.IdExistencia);
        modelBuilder.Entity<MovimientoInventario>().HasKey(m => m.IdMovimiento);
        modelBuilder.Entity<ReservaStock>().HasKey(r => r.IdReserva);

        // Índice único: una existencia por cada combinación Producto - Bodega
        modelBuilder.Entity<Existencia>()
            .HasIndex(e => new { e.IdProducto, e.IdBodega })
            .IsUnique();

        // Relaciones físicas internas en InventarioDB
        modelBuilder.Entity<Existencia>()
            .HasOne(e => e.Bodega)
            .WithMany(b => b.Existencias)
            .HasForeignKey(e => e.IdBodega)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<MovimientoInventario>()
            .HasOne(m => m.Bodega)
            .WithMany(b => b.Movimientos)
            .HasForeignKey(m => m.IdBodega)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ReservaStock>()
            .HasOne(r => r.Bodega)
            .WithMany(b => b.Reservas)
            .HasForeignKey(r => r.IdBodega)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        modelBuilder.Entity<Bodega>().HasData(
            new Bodega { IdBodega = 1, Nombre = "Bodega Principal Norte", Ciudad = "Quito", Direccion = "Av. Galo Plaza Lasso N45-12" },
            new Bodega { IdBodega = 2, Nombre = "Bodega Sucursal Sur", Ciudad = "Guayaquil", Direccion = "Av. de las Américas 1024" },
            new Bodega { IdBodega = 3, Nombre = "Bodega Centro de Distribución", Ciudad = "Cuenca", Direccion = "Parque Industrial Calle C" }
        );

        // Nota: IdProducto=1, 2, 3, 4 son referencias lógicas a CatalogoDB
        modelBuilder.Entity<Existencia>().HasData(
            new Existencia { IdExistencia = 1, IdProducto = 1, IdBodega = 1, Stock = 25 },
            new Existencia { IdExistencia = 2, IdProducto = 1, IdBodega = 2, Stock = 10 },
            new Existencia { IdExistencia = 3, IdProducto = 2, IdBodega = 1, Stock = 18 },
            new Existencia { IdExistencia = 4, IdProducto = 3, IdBodega = 1, Stock = 40 },
            new Existencia { IdExistencia = 5, IdProducto = 3, IdBodega = 2, Stock = 25 },
            new Existencia { IdExistencia = 6, IdProducto = 4, IdBodega = 3, Stock = 100 }
        );

        modelBuilder.Entity<MovimientoInventario>().HasData(
            new MovimientoInventario { IdMovimiento = 1, IdProducto = 1, IdBodega = 1, Tipo = "Entrada", Cantidad = 25, Fecha = new DateTime(2026, 1, 15, 10, 0, 0, DateTimeKind.Utc) },
            new MovimientoInventario { IdMovimiento = 2, IdProducto = 3, IdBodega = 1, Tipo = "Entrada", Cantidad = 40, Fecha = new DateTime(2026, 1, 16, 11, 30, 0, DateTimeKind.Utc) }
        );

        modelBuilder.Entity<ReservaStock>().HasData(
            new ReservaStock { IdReserva = 1, IdProducto = 1, IdBodega = 1, Cantidad = 2, Estado = "Pendiente" }
        );
    }
}
