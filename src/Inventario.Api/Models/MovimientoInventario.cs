using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("MovimientosInventario")]
public class MovimientoInventario
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdMovimiento { get; set; }

    // Referencia logica al producto
    [Required(ErrorMessage = "El IdProducto es obligatorio.")]
    public int IdProducto { get; set; }

    // Relacion fisica con bodega
    [Required(ErrorMessage = "El IdBodega es obligatorio.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El tipo de movimiento es obligatorio (ej: Entrada, Salida, Ajuste).")]
    [StringLength(50, ErrorMessage = "El tipo no puede exceder 50 caracteres.")]
    public string Tipo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Required]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
