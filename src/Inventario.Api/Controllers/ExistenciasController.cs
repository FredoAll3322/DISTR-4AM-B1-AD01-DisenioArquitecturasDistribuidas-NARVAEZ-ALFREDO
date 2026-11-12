using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.DTOs;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExistenciasController : ControllerBase
{
    private readonly InventarioDbContext _context;

    public ExistenciasController(InventarioDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? idProducto, [FromQuery] int? idBodega)
    {
        var query = _context.Existencias.Include(e => e.Bodega).AsNoTracking().AsQueryable();

        if (idProducto.HasValue)
        {
            query = query.Where(e => e.IdProducto == idProducto.Value);
        }

        if (idBodega.HasValue)
        {
            query = query.Where(e => e.IdBodega == idBodega.Value);
        }

        var result = await query.Select(e => new
        {
            e.IdExistencia,
            e.IdProducto,
            e.IdBodega,
            BodegaNombre = e.Bodega != null ? e.Bodega.Nombre : null,
            e.Stock
        }).ToListAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var existencia = await _context.Existencias
            .Include(e => e.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdExistencia == id);

        if (existencia == null)
        {
            return NotFound(new { mensaje = $"Registro de existencia con ID {id} no encontrado." });
        }

        return Ok(new
        {
            existencia.IdExistencia,
            existencia.IdProducto,
            existencia.IdBodega,
            BodegaNombre = existencia.Bodega?.Nombre,
            existencia.Stock
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Existencia>> Create([FromBody] ExistenciaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación FK física interna
        var bodegaExiste = await _context.Bodegas.AnyAsync(b => b.IdBodega == dto.IdBodega);
        if (!bodegaExiste)
        {
            return BadRequest(new { mensaje = $"La bodega con ID {dto.IdBodega} no existe en la base de datos." });
        }

        // Validación de existencia previa
        var existeRegistro = await _context.Existencias
            .AnyAsync(e => e.IdProducto == dto.IdProducto && e.IdBodega == dto.IdBodega);
        if (existeRegistro)
        {
            return BadRequest(new { mensaje = $"Ya existe un registro de stock para el Producto {dto.IdProducto} en la Bodega {dto.IdBodega}. Use PUT para actualizar." });
        }

        var existencia = new Existencia
        {
            IdProducto = dto.IdProducto,
            IdBodega = dto.IdBodega,
            Stock = dto.Stock
        };

        _context.Existencias.Add(existencia);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = existencia.IdExistencia }, existencia);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Existencia>> Update(int id, [FromBody] ExistenciaCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existencia = await _context.Existencias.FindAsync(id);
        if (existencia == null)
        {
            return NotFound(new { mensaje = $"Registro de existencia con ID {id} no encontrado." });
        }

        var bodegaExiste = await _context.Bodegas.AnyAsync(b => b.IdBodega == dto.IdBodega);
        if (!bodegaExiste)
        {
            return BadRequest(new { mensaje = $"La bodega con ID {dto.IdBodega} no existe." });
        }

        existencia.IdProducto = dto.IdProducto;
        existencia.IdBodega = dto.IdBodega;
        existencia.Stock = dto.Stock;

        await _context.SaveChangesAsync();
        return Ok(existencia);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var existencia = await _context.Existencias.FindAsync(id);
        if (existencia == null)
        {
            return NotFound(new { mensaje = $"Registro de existencia con ID {id} no encontrado." });
        }

        _context.Existencias.Remove(existencia);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Registro de existencia con ID {id} eliminado exitosamente." });
    }
}
