using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Inventario.Api.Data;
using Inventario.Api.Models;
using Inventario.Api.DTOs;
using Inventario.Api.Services;

namespace Inventario.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MovimientosController : ControllerBase
{
    private readonly InventarioDbContext _context;
    private readonly CatalogoApiClient _catalogoApiClient;

    public MovimientosController(InventarioDbContext context, CatalogoApiClient catalogoApiClient)
    {
        _context = context;
        _catalogoApiClient = catalogoApiClient;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? idBodega, [FromQuery] int? idProducto)
    {
        var query = _context.Movimientos.Include(m => m.Bodega).AsNoTracking().AsQueryable();

        if (idBodega.HasValue)
        {
            query = query.Where(m => m.IdBodega == idBodega.Value);
        }

        if (idProducto.HasValue)
        {
            query = query.Where(m => m.IdProducto == idProducto.Value);
        }

        var list = await query.OrderByDescending(m => m.Fecha).Select(m => new
        {
            m.IdMovimiento,
            m.IdProducto,
            m.IdBodega,
            BodegaNombre = m.Bodega != null ? m.Bodega.Nombre : null,
            m.Tipo,
            m.Cantidad,
            m.Fecha
        }).ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var m = await _context.Movimientos
            .Include(x => x.Bodega)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdMovimiento == id);

        if (m == null)
        {
            return NotFound(new { mensaje = $"Movimiento con ID {id} no encontrado." });
        }

        return Ok(new
        {
            m.IdMovimiento,
            m.IdProducto,
            m.IdBodega,
            BodegaNombre = m.Bodega?.Nombre,
            m.Tipo,
            m.Cantidad,
            m.Fecha
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] MovimientoCreateDto dto)
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

        var tipo = dto.Tipo.Trim();
        var tiposPermitidos = new[] { "Entrada", "Salida", "Ajuste" };
        if (!tiposPermitidos.Contains(tipo, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { mensaje = "El tipo de movimiento debe ser 'Entrada', 'Salida' o 'Ajuste'." });
        }

        var bodegaExiste = await _context.Bodegas.AnyAsync(b => b.IdBodega == dto.IdBodega);
        if (!bodegaExiste)
        {
            return BadRequest(new { mensaje = $"La bodega con ID {dto.IdBodega} no existe." });
        }

        // Buscar existencia actual
        var existencia = await _context.Existencias
            .FirstOrDefaultAsync(e => e.IdProducto == dto.IdProducto && e.IdBodega == dto.IdBodega);

        if (tipo.Equals("Salida", StringComparison.OrdinalIgnoreCase))
        {
            if (existencia == null || existencia.Stock < dto.Cantidad)
            {
                var stockActual = existencia?.Stock ?? 0;
                return BadRequest(new { mensaje = $"Stock insuficiente. Stock actual: {stockActual}, solicitado: {dto.Cantidad}." });
            }
            existencia.Stock -= dto.Cantidad;
        }
        else if (tipo.Equals("Entrada", StringComparison.OrdinalIgnoreCase))
        {
            if (existencia == null)
            {
                existencia = new Existencia
                {
                    IdProducto = dto.IdProducto,
                    IdBodega = dto.IdBodega,
                    Stock = dto.Cantidad
                };
                _context.Existencias.Add(existencia);
            }
            else
            {
                existencia.Stock += dto.Cantidad;
            }
        }
        else if (tipo.Equals("Ajuste", StringComparison.OrdinalIgnoreCase))
        {
            if (existencia == null)
            {
                existencia = new Existencia
                {
                    IdProducto = dto.IdProducto,
                    IdBodega = dto.IdBodega,
                    Stock = dto.Cantidad
                };
                _context.Existencias.Add(existencia);
            }
            else
            {
                existencia.Stock = dto.Cantidad;
            }
        }

        var movimiento = new MovimientoInventario
        {
            IdProducto = dto.IdProducto,
            IdBodega = dto.IdBodega,
            Tipo = tipo,
            Cantidad = dto.Cantidad,
            Fecha = DateTime.UtcNow
        };

        _context.Movimientos.Add(movimiento);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = movimiento.IdMovimiento }, movimiento);
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
