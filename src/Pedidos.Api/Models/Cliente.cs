using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Pedidos.Api.Models;

[Table("Clientes")]
public class Cliente
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCliente { get; set; }

    [Required(ErrorMessage = "Los nombres del cliente son obligatorios.")]
    [StringLength(150, ErrorMessage = "Los nombres no pueden exceder 150 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [StringLength(20, ErrorMessage = "La cédula no puede exceder 20 caracteres.")]
    public string Cedula { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato de correo no es válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede exceder 100 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    // Relación física interna en PedidosDB
    [JsonIgnore]
    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
