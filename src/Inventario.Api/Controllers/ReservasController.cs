using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.DTOs;
using Inventario.Api.Services;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReservasController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoApiClient _catalogoApiClient;

    public ReservasController(InventarioDbContext context, CatalogoApiClient catalogoApiClient)
    {
        _context = context;
        _catalogoApiClient = catalogoApiClient;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] string? estado)
    {
        var query = _context.Reservas.Include(r => r.Bodega).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(r => r.Estado.ToLower() == estado.ToLower());
        }

        var list = await query.Select(r => new
        {
            r.IdReserva,
            r.IdProducto,
            r.IdBodega,
            BodegaNombre = r.Bodega != null ? r.Bodega.Nombre : null,
            r.Cantidad,
            r.Estado
        }).ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var r = await _context.Reservas
            .Include(x => x.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdReserva == id);

        if (r == null)
        {
            return NotFound(new { mensaje = $"Reserva con ID {id} no encontrada." });
        }

        return Ok(new
        {
            r.IdReserva,
            r.IdProducto,
            r.IdBodega,
            BodegaNombre = r.Bodega?.Nombre,
            r.Cantidad,
            r.Estado
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] ReservaCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var validacionProducto = await ValidarProductoAsync(dto.IdProducto);
        if (validacionProducto != null)
        {
            return validacionProducto;
        }

        var bodegaExiste = await _context.Bodegas.AnyAsync(b => b.IdBodega == dto.IdBodega);
        if (!bodegaExiste)
        {
            return BadRequest(new { mensaje = $"La bodega con ID {dto.IdBodega} no existe." });
        }

        var existencia = await _context.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == dto.IdProducto && e.IdBodega == dto.IdBodega);

        if (existencia == null || existencia.Stock < dto.Cantidad)
        {
            var stockActual = existencia?.Stock ?? 0;
            return BadRequest(new { mensaje = $"No hay suficiente stock para reservar. Stock disponible: {stockActual}, solicitado: {dto.Cantidad}." });
        }

        var reserva = new ReservaStock
        {
            IdProducto = dto.IdProducto,
            IdBodega = dto.IdBodega,
            Cantidad = dto.Cantidad,
            Estado = string.IsNullOrWhiteSpace(dto.Estado) ? "Pendiente" : dto.Estado
        };

        _context.Reservas.Add(reserva);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = reserva.IdReserva }, reserva);
    }

    [HttpPatch("{id}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ReservaStock>> UpdateEstado(int id, [FromBody] ReservaUpdateEstadoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null)
        {
            return NotFound(new { mensaje = $"Reserva con ID {id} no encontrada." });
        }

        var estadosPermitidos = new[] { "Pendiente", "Confirmada", "Cancelada" };
        if (!estadosPermitidos.Contains(dto.Estado, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { mensaje = "El estado debe ser 'Pendiente', 'Confirmada' o 'Cancelada'." });
        }

        reserva.Estado = dto.Estado;
        await _context.SaveChangesAsync();

        return Ok(reserva);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var reserva = await _context.Reservas.FindAsync(id);
        if (reserva == null)
        {
            return NotFound(new { mensaje = $"Reserva con ID {id} no encontrada." });
        }

        _context.Reservas.Remove(reserva);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Reserva con ID {id} eliminada exitosamente." });
    }

    private async Task<IActionResult?> ValidarProductoAsync(int idProducto)
    {
        var estadoProducto = await _catalogoApiClient.ConsultarProductoAsync(idProducto, HttpContext.RequestAborted);
        return estadoProducto switch
        {
            EstadoProductoCatalogo.NoExiste => BadRequest(new { mensaje = $"El producto con ID {idProducto} no existe en Catalogo.Api." }),
            EstadoProductoCatalogo.NoDisponible => StatusCode(StatusCodes.Status503ServiceUnavailable,
                new { mensaje = "No se pudo validar el producto porque Catalogo.Api no está disponible." }),
            _ => null
        };
    }
}
