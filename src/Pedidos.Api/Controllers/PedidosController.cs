using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.DTOs;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly PedidosDbContext _context;

    public PedidosController(PedidosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll([FromQuery] int? idCliente, [FromQuery] string? estado)
    {
        var query = _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .AsNoTracking()
            .AsQueryable();

        if (idCliente.HasValue)
        {
            query = query.Where(p => p.IdCliente == idCliente.Value);
        }

        if (!string.IsNullOrWhiteSpace(estado))
        {
            query = query.Where(p => p.Estado.ToLower() == estado.ToLower());
        }

        var list = await query.OrderByDescending(p => p.Fecha).Select(p => new
        {
            p.IdPedido,
            p.IdCliente,
            ClienteNombre = p.Cliente != null ? p.Cliente.Nombres : null,
            ClienteCedula = p.Cliente != null ? p.Cliente.Cedula : null,
            p.Fecha,
            p.Total,
            p.Estado,
            Detalles = p.Detalles.Select(d => new
            {
                d.IdDetalle,
                d.IdProducto,
                d.Cantidad,
                d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            })
        }).ToListAsync();

        return Ok(list);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetById(int id)
    {
        var p = await _context.Pedidos
            .Include(p => p.Cliente)
            .Include(p => p.Detalles)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPedido == id);

        if (p == null)
        {
            return NotFound(new { mensaje = $"Pedido con ID {id} no encontrado." });
        }

        return Ok(new
        {
            p.IdPedido,
            p.IdCliente,
            ClienteNombre = p.Cliente?.Nombres,
            ClienteCedula = p.Cliente?.Cedula,
            ClienteCorreo = p.Cliente?.Correo,
            p.Fecha,
            p.Total,
            p.Estado,
            Detalles = p.Detalles.Select(d => new
            {
                d.IdDetalle,
                d.IdProducto,
                d.Cantidad,
                d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            })
        });
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<object>> Create([FromBody] PedidoCreateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Validación FK física interna
        var cliente = await _context.Clientes.FindAsync(dto.IdCliente);
        if (cliente == null)
        {
            return BadRequest(new { mensaje = $"El cliente con ID {dto.IdCliente} no existe en la base de datos." });
        }

        if (dto.Detalles == null || dto.Detalles.Count == 0)
        {
            return BadRequest(new { mensaje = "El pedido debe contener al menos un producto." });
        }

        var pedido = new Pedido
        {
            IdCliente = dto.IdCliente,
            Fecha = DateTime.UtcNow,
            Estado = "Pendiente",
            Total = dto.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario)
        };

        foreach (var item in dto.Detalles)
        {
            pedido.Detalles.Add(new DetallePedido
            {
                IdProducto = item.IdProducto,
                Cantidad = item.Cantidad,
                PrecioUnitario = item.PrecioUnitario
            });
        }

        _context.Pedidos.Add(pedido);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = pedido.IdPedido }, new
        {
            pedido.IdPedido,
            pedido.IdCliente,
            pedido.Fecha,
            pedido.Total,
            pedido.Estado,
            Detalles = pedido.Detalles.Select(d => new
            {
                d.IdDetalle,
                d.IdProducto,
                d.Cantidad,
                d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            })
        });
    }

    [HttpPatch("{id}/estado")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Pedido>> UpdateEstado(int id, [FromBody] PedidoUpdateEstadoDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var pedido = await _context.Pedidos.FindAsync(id);
        if (pedido == null)
        {
            return NotFound(new { mensaje = $"Pedido con ID {id} no encontrado." });
        }

        var estadosValidos = new[] { "Pendiente", "Procesado", "Completado", "Cancelado" };
        if (!estadosValidos.Contains(dto.Estado, StringComparer.OrdinalIgnoreCase))
        {
            return BadRequest(new { mensaje = "El estado debe ser: 'Pendiente', 'Procesado', 'Completado' o 'Cancelado'." });
        }

        pedido.Estado = dto.Estado;
        await _context.SaveChangesAsync();

        return Ok(pedido);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var pedido = await _context.Pedidos
            .Include(p => p.Detalles)
            .FirstOrDefaultAsync(p => p.IdPedido == id);

        if (pedido == null)
        {
            return NotFound(new { mensaje = $"Pedido con ID {id} no encontrado." });
        }

        _context.Pedidos.Remove(pedido);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Pedido con ID {id} y sus detalles han sido eliminados exitosamente." });
    }
}
