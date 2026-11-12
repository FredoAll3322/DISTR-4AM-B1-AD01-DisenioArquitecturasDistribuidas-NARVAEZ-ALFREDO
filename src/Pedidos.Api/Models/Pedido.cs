using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Pedidos.Api.Models;

[Table("Pedidos")]
public class Pedido
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPedido { get; set; }

    /// <summary>
    /// LLAVE FORÁNEA FÍSICA: Referencia interna a la tabla Clientes dentro de PedidosDB.
    /// </summary>
    [Required(ErrorMessage = "El IdCliente es obligatorio.")]
    public int IdCliente { get; set; }

    [Required]
    public DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Required]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    [Required(ErrorMessage = "El estado del pedido es obligatorio (ej: Pendiente, Procesado, Completado, Cancelado).")]
    [StringLength(50, ErrorMessage = "El estado no puede exceder 50 caracteres.")]
    public string Estado { get; set; } = "Pendiente";

    [ForeignKey(nameof(IdCliente))]
    public Cliente? Cliente { get; set; }

    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}
