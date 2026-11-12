using System.ComponentModel.DataAnnotations;

namespace Pedidos.Api.DTOs;

public class ClienteCreateUpdateDto
{
    [Required(ErrorMessage = "Los nombres del cliente son obligatorios.")]
    [StringLength(150, ErrorMessage = "Los nombres no pueden exceder 150 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres.")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo electrónico no tiene un formato válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres.")]
    public string Correo { get; set; } = string.Empty;
}

public class DetallePedidoItemDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 10000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El precio unitario es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    public decimal PrecioUnitario { get; set; }
}

public class PedidoCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El IdCliente debe ser mayor a 0.")]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "El pedido debe contener al menos un detalle de producto.")]
    [MinLength(1, ErrorMessage = "Debe incluir al menos un producto en el pedido.")]
    public List<DetallePedidoItemDto> Detalles { get; set; } = new();
}

public class PedidoUpdateEstadoDto
{
    [Required(ErrorMessage = "El estado es obligatorio (ej: Pendiente, Procesado, Completado, Cancelado).")]
    public string Estado { get; set; } = string.Empty;
}
