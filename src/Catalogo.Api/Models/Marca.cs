using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

[Table("Marcas")]
public class Marca
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país de origen es obligatorio.")]
    [StringLength(100, ErrorMessage = "El país de origen no puede exceder 100 caracteres.")]
    public string PaisOrigen { get; set; } = string.Empty;

    // Relación de navegación interna física
    [JsonIgnore]
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
