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

    /// <summary>
    /// LLAVE FORÁNEA FÍSICA: Referencia interna a la tabla Pedidos dentro de PedidosDB.
    /// </summary>
    [Required]
    public int IdPedido { get; set; }

    /// <summary>
    /// LLAVE FORÁNEA LÓGICA: Referencia al IdProducto de Catalogo.Api (CatalogoDB).
    /// En microservicios no existe restricción de integridad referencial a nivel de base de datos SQL.
    /// </summary>
    [Required(ErrorMessage = "El IdProducto (referencia lógica a Catalogo) es obligatorio.")]
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
