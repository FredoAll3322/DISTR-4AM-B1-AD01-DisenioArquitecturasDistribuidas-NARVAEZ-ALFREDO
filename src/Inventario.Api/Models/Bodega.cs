using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Inventario.Api.Models;

[Table("Bodegas")]
public class Bodega
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El nombre de la bodega es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(100, ErrorMessage = "La ciudad no puede exceder 100 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;

    [Required(ErrorMessage = "La dirección es obligatoria.")]
    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres.")]
    public string Direccion { get; set; } = string.Empty;

    // Relaciones físicas internas en InventarioDB
    [JsonIgnore]
    public ICollection<Existencia> Existencias { get; set; } = new List<Existencia>();

    [JsonIgnore]
    public ICollection<MovimientoInventario> Movimientos { get; set; } = new List<MovimientoInventario>();

    [JsonIgnore]
    public ICollection<ReservaStock> Reservas { get; set; } = new List<ReservaStock>();
}
