using Microsoft.EntityFrameworkCore;
using Catalogo.Api.Models;

namespace Catalogo.Api.Data;

public class CatalogoDbContext : DbContext
{
    public CatalogoDbContext(DbContextOptions<CatalogoDbContext> options) : base(options)
    {
    }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Claves primarias
        modelBuilder.Entity<Categoria>().HasKey(c => c.IdCategoria);
        modelBuilder.Entity<Marca>().HasKey(m => m.IdMarca);
        modelBuilder.Entity<Producto>().HasKey(p => p.IdProducto);

        // SKU único en Producto
        modelBuilder.Entity<Producto>()
            .HasIndex(p => p.SKU)
            .IsUnique();

        // Relaciones físicas internas en CatalogoDB
        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Categoria)
            .WithMany(c => c.Productos)
            .HasForeignKey(p => p.IdCategoria)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Producto>()
            .HasOne(p => p.Marca)
            .WithMany(m => m.Productos)
            .HasForeignKey(p => p.IdMarca)
            .OnDelete(DeleteBehavior.Restrict);

        // Seed data
        modelBuilder.Entity<Categoria>().HasData(
            new Categoria { IdCategoria = 1, Nombre = "Laptops & Portátiles", Descripcion = "Equipos de cómputo portátiles para trabajo y gaming" },
            new Categoria { IdCategoria = 2, Nombre = "Smartphones & Tablets", Descripcion = "Dispositivos móviles inteligentes" },
            new Categoria { IdCategoria = 3, Nombre = "Periféricos & Accesorios", Descripcion = "Teclados, mouse, monitores y cables" }
        );

        modelBuilder.Entity<Marca>().HasData(
            new Marca { IdMarca = 1, Nombre = "Dell", PaisOrigen = "Estados Unidos" },
            new Marca { IdMarca = 2, Nombre = "Lenovo", PaisOrigen = "China" },
            new Marca { IdMarca = 3, Nombre = "Samsung", PaisOrigen = "Corea del Sur" },
            new Marca { IdMarca = 4, Nombre = "Logitech", PaisOrigen = "Suiza" }
        );

        modelBuilder.Entity<Producto>().HasData(
            new Producto { IdProducto = 1, IdCategoria = 1, IdMarca = 1, Nombre = "Dell XPS 15", SKU = "DELL-XPS15-001", Precio = 1850.00m, Estado = "Activo" },
            new Producto { IdProducto = 2, IdCategoria = 1, IdMarca = 2, Nombre = "Lenovo ThinkPad X1", SKU = "LEN-TPX1-002", Precio = 1600.00m, Estado = "Activo" },
            new Producto { IdProducto = 3, IdCategoria = 2, IdMarca = 3, Nombre = "Samsung Galaxy S24 Ultra", SKU = "SAM-S24U-003", Precio = 1299.99m, Estado = "Activo" },
            new Producto { IdProducto = 4, IdCategoria = 3, IdMarca = 4, Nombre = "Mouse Inalámbrico MX Master 3S", SKU = "LOG-MX3S-004", Precio = 99.50m, Estado = "Activo" }
        );
    }
}
