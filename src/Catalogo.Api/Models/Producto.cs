using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Catalogo.Api.Models;

[Table("Productos")]
public class Producto
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdProducto { get; set; }

    [Required(ErrorMessage = "La categoría es obligatoria.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "La marca es obligatoria.")]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede exceder 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(50, ErrorMessage = "El SKU no puede exceder 50 caracteres.")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    [Column(TypeName = "decimal(18,2)")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio.")]
    [StringLength(20, ErrorMessage = "El estado no puede exceder 20 caracteres.")]
    public string Estado { get; set; } = "Activo";

    // Propiedades de navegación física interna dentro del microservicio
    [ForeignKey(nameof(IdCategoria))]
    public Categoria? Categoria { get; set; }

    [ForeignKey(nameof(IdMarca))]
    public Marca? Marca { get; set; }
}
