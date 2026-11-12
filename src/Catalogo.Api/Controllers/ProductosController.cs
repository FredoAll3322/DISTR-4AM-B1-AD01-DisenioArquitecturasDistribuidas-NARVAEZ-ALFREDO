using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Catalogo.Api.DTOs;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductosController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public ProductosController(CatalogoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] string? estado)
    {
        var query = _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(p => p.Estado.ToLower() == estado.ToLower());
        }

        var productos = await query.Select(p => new
        {
            p.IdProducto,
            p.IdCategoria,
            CategoriaNombre = p.Categoria != null ? p.Categoria.Nombre : null,
            p.IdMarca,
            MarcaNombre = p.Marca != null ? p.Marca.Nombre : null,
            p.Nombre,
            p.SKU,
            p.Precio,
            p.Estado
        }).ToListAsync();

        return Ok(productos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var producto = await _context.Productos
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProducto == id);

        if (producto == null)
        {
            return NotFound(new { mensaje = $"Producto con ID {id} no encontrado." });
        }

        return Ok(new
        {
            producto.IdProducto,
            producto.IdCategoria,
            CategoriaNombre = producto.Categoria?.Nombre,
            producto.IdMarca,
            MarcaNombre = producto.Marca?.Nombre,
            producto.Nombre,
            producto.SKU,
            producto.Precio,
            producto.Estado
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Producto>> Create([FromBody] ProductoCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación de FK física: Categoria
        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria);
        if (!categoriaExiste)
        {
            return BadRequest(new { mensaje = $"La categoría con ID {dto.IdCategoria} no existe en la base de datos." });
        }

        // Validación de FK física: Marca
        var marcaExiste = await _context.Marcas.AnyAsync(m => m.IdMarca == dto.IdMarca);
        if (!marcaExiste)
        {
            return BadRequest(new { mensaje = $"La marca con ID {dto.IdMarca} no existe en la base de datos." });
        }

        // Validación de SKU único
        var skuExiste = await _context.Productos.AnyAsync(p => p.SKU.ToLower() == dto.SKU.ToLower());
        if (skuExiste)
        {
            return BadRequest(new { mensaje = $"Ya existe un producto registrado con el SKU '{dto.SKU}'." });
        }

        var producto = new Producto
        {
            IdCategoria = dto.IdCategoria,
            IdMarca = dto.IdMarca,
            Nombre = dto.Nombre,
            SKU = dto.SKU.ToUpper().Trim(),
            Precio = dto.Precio,
            Estado = dto.Estado
        };

        _context.Productos.Add(producto);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = producto.IdProducto }, producto);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Producto>> Update(int id, [FromBody] ProductoCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"Producto con ID {id} no encontrado." });
        }

        // Validación FK física Categoria
        var categoriaExiste = await _context.Categorias.AnyAsync(c => c.IdCategoria == dto.IdCategoria);
        if (!categoriaExiste)
        {
            return BadRequest(new { mensaje = $"La categoría con ID {dto.IdCategoria} no existe." });
        }

        // Validación FK física Marca
        var marcaExiste = await _context.Marcas.AnyAsync(m => m.IdMarca == dto.IdMarca);
        if (!marcaExiste)
        {
            return BadRequest(new { mensaje = $"La marca con ID {dto.IdMarca} no existe." });
        }

        // Validación SKU único para otro producto
        var skuExiste = await _context.Productos.AnyAsync(p => p.SKU.ToLower() == dto.SKU.ToLower() && p.IdProducto != id);
        if (skuExiste)
        {
            return BadRequest(new { mensaje = $"El SKU '{dto.SKU}' ya se encuentra utilizado por otro producto." });
        }

        producto.IdCategoria = dto.IdCategoria;
        producto.IdMarca = dto.IdMarca;
        producto.Nombre = dto.Nombre;
        producto.SKU = dto.SKU.ToUpper().Trim();
        producto.Precio = dto.Precio;
        producto.Estado = dto.Estado;

        await _context.SaveChangesAsync();
        return Ok(producto);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var producto = await _context.Productos.FindAsync(id);
        if (producto == null)
        {
            return NotFound(new { mensaje = $"Producto con ID {id} no encontrado." });
        }

        _context.Productos.Remove(producto);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Producto con ID {id} eliminado exitosamente." });
    }
}
