using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Pedidos.Api.Data;
using Pedidos.Api.Models;
using Pedidos.Api.DTOs;

namespace Pedidos.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientesController : ControllerBase
{
    private readonly PedidosDbContext _context;

    public ClientesController(PedidosDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<Cliente>>> GetAll()
    {
        return Ok(await _context.Clientes.AsNoTracking().ToListAsync());
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Cliente>> GetById(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound(new { mensaje = $"Cliente con ID {id} no encontrado." });
        }
        return Ok(cliente);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Cliente>> Create([FromBody] ClienteCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existeCedula = await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula.Trim());
        if (existeCedula)
        {
            return BadRequest(new { mensaje = $"Ya existe un cliente registrado con la cédula '{dto.Cedula}'." });
        }

        var cliente = new Cliente
        {
            Nombres = dto.Nombres,
            Cedula = dto.Cedula.Trim(),
            Correo = dto.Correo.Trim().ToLower()
        };

        _context.Clientes.Add(cliente);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = cliente.IdCliente }, cliente);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Cliente>> Update(int id, [FromBody] ClienteCreateUpdateDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound(new { mensaje = $"Cliente con ID {id} no encontrado." });
        }

        var existeCedula = await _context.Clientes.AnyAsync(c => c.Cedula == dto.Cedula.Trim() && c.IdCliente != id);
        if (existeCedula)
        {
            return BadRequest(new { mensaje = $"La cédula '{dto.Cedula}' ya está registrada con otro cliente." });
        }

        cliente.Nombres = dto.Nombres;
        cliente.Cedula = dto.Cedula.Trim();
        cliente.Correo = dto.Correo.Trim().ToLower();

        await _context.SaveChangesAsync();
        return Ok(cliente);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(int id)
    {
        var cliente = await _context.Clientes.FindAsync(id);
        if (cliente == null)
        {
            return NotFound(new { mensaje = $"Cliente con ID {id} no encontrado." });
        }

        var tienePedidos = await _context.Pedidos.AnyAsync(p => p.IdCliente == id);
        if (tienePedidos)
        {
            return BadRequest(new { mensaje = "No se puede eliminar el cliente porque tiene órdenes de compra registradas." });
        }

        _context.Clientes.Remove(cliente);
        await _context.SaveChangesAsync();

        return Ok(new { mensaje = $"Cliente con ID {id} eliminado exitosamente." });
    }
}
