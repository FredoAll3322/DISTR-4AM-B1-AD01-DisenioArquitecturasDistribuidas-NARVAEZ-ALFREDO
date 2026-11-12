using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalogo.Api.Data;
using Catalogo.Api.Models;
using Catalogo.Api.DTOs;

namespace Catalogo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MarcasController : ControllerBase
{
    private readonly CatalogoDbContext _context;

    public MarcasController(CatalogoDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Marca>>> GetAll()
    {
        return Ok(await _context.Marcas.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Marca>> GetById(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            return NotFound(new { mensaje = $"Marca con ID {id} no encontrada." });
        }
        return Ok(marca);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Marca>> Create([FromBody] MarcaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existeNombre = await _context.Marcas.AnyAsync(m => m.Nombre.ToLower() == dto.Nombre.ToLower());
        if (existeNombre)
        {
            return BadRequest(new { mensaje = $"Ya existe una marca con el nombre '{dto.Nombre}'." });
        }

        var marca = new Marca
        {
            Nombre = dto.Nombre,
            PaisOrigen = dto.PaisOrigen
        };

        _context.Marcas.Add(marca);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = marca.IdMarca }, marca);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Marca>> Update(int id, [FromBody] MarcaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            return NotFound(new { mensaje = $"Marca con ID {id} no encontrada." });
        }

        var existeNombre = await _context.Marcas.AnyAsync(m => m.Nombre.ToLower() == dto.Nombre.ToLower() && m.IdMarca != id);
        if (existeNombre)
        {
            return BadRequest(new { mensaje = $"Ya existe otra marca con el nombre '{dto.Nombre}'." });
        }

        marca.Nombre = dto.Nombre;
        marca.PaisOrigen = dto.PaisOrigen;

        await _context.SaveChangesAsync();
        return Ok(marca);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var marca = await _context.Marcas.FindAsync(id);
        if (marca == null)
        {
            return NotFound(new { mensaje = $"Marca con ID {id} no encontrada." });
        }

        var tieneProductos = await _context.Productos.AnyAsync(p => p.IdMarca == id);
        if (tieneProductos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la marca porque tiene productos asociados." });
        }

        _context.Marcas.Remove(marca);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Marca con ID {id} eliminada exitosamente." });
    }
}
