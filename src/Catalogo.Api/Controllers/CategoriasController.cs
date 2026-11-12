using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Catalogo.Api.DTOs;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriasController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public CategoriasController(CatalogoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Categoria>>> GetAll()
    {
        return Ok(await _context.Categorias.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Categoria>> GetById(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
        }
        return Ok(categoria);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Categoria>> Create([FromBody] CategoriaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existeNombre = await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower());
        if (existeNombre)
        {
            return BadRequest(new { mensaje = $"Ya existe una categoría con el nombre '{dto.Nombre}'." });
        }

        var categoria = new Categoria
        {
            Nombre = dto.Nombre,
            Descripcion = dto.Descripcion
        };

        _context.Categorias.Add(categoria);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = categoria.IdCategoria }, categoria);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Categoria>> Update(int id, [FromBody] CategoriaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
        }

        var existeNombre = await _context.Categorias.AnyAsync(c => c.Nombre.ToLower() == dto.Nombre.ToLower() && c.IdCategoria != id);
        if (existeNombre)
        {
            return BadRequest(new { mensaje = $"Ya existe otra categoría con el nombre '{dto.Nombre}'." });
        }

        categoria.Nombre = dto.Nombre;
        categoria.Descripcion = dto.Descripcion;

        await _context.SaveChangesAsync();
        return Ok(categoria);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var categoria = await _context.Categorias.FindAsync(id);
        if (categoria == null)
        {
            return NotFound(new { mensaje = $"Categoría con ID {id} no encontrada." });
        }

        var tieneProductos = await _context.Productos.AnyAsync(p => p.IdCategoria == id);
        if (tieneProductos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la categoría porque tiene productos asociados." });
        }

        _context.Categorias.Remove(categoria);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Categoría con ID {id} eliminada exitosamente." });
    }
}
