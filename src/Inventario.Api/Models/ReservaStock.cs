using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("ReservasStock")]
public class ReservaStock
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdReserva { get; set; }

    /// <summary>
    /// LLAVE FORÁNEA LÓGICA: Referencia al IdProducto de Catalogo.Api.
    /// </summary>
    [Required(ErrorMessage = "El IdProducto (referencia lógica a Catalogo) es obligatorio.")]
    public int IdProducto { get; set; }

    /// <summary>
    /// LLAVE FORÁNEA FÍSICA: Referencia interna a Bodegas en InventarioDB.
    /// </summary>
    [Required(ErrorMessage = "El IdBodega es obligatorio.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "La cantidad a reservar es obligatoria.")]
    [Range(1, 100000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio (ej: Pendiente, Confirmada, Cancelada).")]
    [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres.")]
    public string Estado { get; set; } = "Pendiente";

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
