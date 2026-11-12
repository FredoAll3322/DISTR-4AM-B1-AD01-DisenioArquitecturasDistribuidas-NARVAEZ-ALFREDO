using System.ComponentModel.DataAnnotations;

namespace Inventario.Api.DTOs;

public class BodegaCreateUpdateDto
{
    [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;
}

public class ExistenciaCreateUpdateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser mayor a 0.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock debe ser 0 o positivo.")]
    public int Stock { get; set; }
}

public class MovimientoCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser mayor a 0.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El tipo de movimiento es obligatorio (Entrada, Salida, Ajuste).")]
    public string Tipo { get; set; } = "Entrada";

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }
}

public class ReservaCreateDto
{
    [Range(1, int.MaxValue, ErrorMessage = "El IdProducto debe ser mayor a 0.")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El IdBodega debe ser mayor a 0.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    public string Estado { get; set; } = "Pendiente";
}

public class ReservaUpdateEstadoDto
{
    [Required(ErrorMessage = "El nuevo estado es obligatorio (ej: Pendiente, Confirmada, Cancelada).")]
    public string Estado { get; set; } = string.Empty;
}
