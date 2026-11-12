using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("DetallesPedido")]
public class DetallePedido
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDetalle { get; set; }

    // Llave foranea fisica al pedido
    [Required]
    public int IdPedido { get; set; }

    // Llave foranea logica: referencia al producto de Catalogo.Api
    [Required(ErrorMessage = "El IdProducto es obligatorio.")]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "La cantidad es obligatoria.")]
    [Range(1, 10000, ErrorMessage = "La cantidad debe ser mayor a 0.")]
    public int Cantidad { get; set; }

    [Required(ErrorMessage = "El precio unitario es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio unitario debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioUnitario { get; set; }

    [ForeignKey(nameof(IdPedido))]
    [JsonIgnore]
    public Pedido? Pedido { get; set; }
}
