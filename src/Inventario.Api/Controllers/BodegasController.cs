using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.DTOs;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BodegasController : ControllerBase
{
    private readonly InventarioDbContext _context;

    public BodegasController(InventarioDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Bodega>>> GetAll()
    {
        return Ok(await _context.Bodegas.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Bodega>> GetById(int id)
    {
        var bodega = await _context.Bodegas.FindAsync(id);
        if (bodega == null)
        {
            return NotFound(new { mensaje = $"Bodega con ID {id} no encontrada." });
        }
        return Ok(bodega);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Bodega>> Create([FromBody] BodegaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existe = await _context.Bodegas.AnyAsync(b => b.Nombre.ToLower() == dto.Nombre.ToLower() && b.Ciudad.ToLower() == dto.Ciudad.ToLower());
        if (existe)
        {
            return BadRequest(new { mensaje = $"Ya existe una bodega con el nombre '{dto.Nombre}' en la ciudad '{dto.Ciudad}'." });
        }

        var bodega = new Bodega
        {
            Nombre = dto.Nombre,
            Ciudad = dto.Ciudad,
            Direccion = dto.Direccion
        };

        _context.Bodegas.Add(bodega);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = bodega.IdBodega }, bodega);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Bodega>> Update(int id, [FromBody] BodegaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var bodega = await _context.Bodegas.FindAsync(id);
        if (bodega == null)
        {
            return NotFound(new { mensaje = $"Bodega con ID {id} no encontrada." });
        }

        bodega.Nombre = dto.Nombre;
        bodega.Ciudad = dto.Ciudad;
        bodega.Direccion = dto.Direccion;

        await _context.SaveChangesAsync();
        return Ok(bodega);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var bodega = await _context.Bodegas.FindAsync(id);
        if (bodega == null)
        {
            return NotFound(new { mensaje = $"Bodega con ID {id} no encontrada." });
        }

        var tieneExistencias = await _context.Existencias.AnyAsync(e => e.IdBodega == id && e.Stock > 0);
        if (tieneExistencias)
        {
            return BadRequest(new { mensaje = "No se puede eliminar la bodega porque tiene existencias con stock." });
        }

        _context.Bodegas.Remove(bodega);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Bodega con ID {id} eliminada exitosamente." });
    }
}
