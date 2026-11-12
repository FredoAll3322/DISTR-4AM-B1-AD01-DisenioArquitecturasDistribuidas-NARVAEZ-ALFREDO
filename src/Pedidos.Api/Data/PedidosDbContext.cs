using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Models;

namespace Pedidos.Api.Data;

public class PedidosDbContext : DbContext
{
    public PedidosDbContext(DbContextOptions<PedidosDbContext> options) : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Claves primarias
        modelBuilder.Entity<Cliente>().HasKey(c => c.IdCliente);
        modelBuilder.Entity<Pedido>().HasKey(p => p.IdPedido);
        modelBuilder.Entity<DetallePedido>().HasKey(d => d.IdDetalle);

        // Cédula única
        modelBuilder.Entity<Cliente>()
            .HasIndex(c => c.Cedula)
            .IsUnique();

        // Relaciones físicas internas en PedidosDB
        modelBuilder.Entity<Pedido>()
            .HasOne(p => p.Cliente)
            .WithMany(c => c.Pedidos)
            .HasForeignKey(p => p.IdCliente)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<DetallePedido>()
            .HasOne(d => d.Pedido)
            .WithMany(p => p.Detalles)
            .HasForeignKey(d => d.IdPedido)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed data
        modelBuilder.Entity<Cliente>().HasData(
            new Cliente { IdCliente = 1, Nombres = "Juan Carlos Pérez", Cedula = "1712345678", Correo = "juan.perez@cordillera.edu.ec" },
            new Cliente { IdCliente = 2, Nombres = "María Elena Gómez", Cedula = "0923456781", Correo = "maria.gomez@empresa.com" }
        );

        modelBuilder.Entity<Pedido>().HasData(
            new Pedido { IdPedido = 1, IdCliente = 1, Fecha = new DateTime(2026, 2, 1, 14, 0, 0, DateTimeKind.Utc), Total = 1949.50m, Estado = "Completado" }
        );

        // IdProducto=1 (Dell XPS 15 a 1850.00) e IdProducto=4 (Mouse MX Master a 99.50) => Total = 1949.50
        modelBuilder.Entity<DetallePedido>().HasData(
            new DetallePedido { IdDetalle = 1, IdPedido = 1, IdProducto = 1, Cantidad = 1, PrecioUnitario = 1850.00m },
            new DetallePedido { IdDetalle = 2, IdPedido = 1, IdProducto = 4, Cantidad = 1, PrecioUnitario = 99.50m }
        );
    }
}
