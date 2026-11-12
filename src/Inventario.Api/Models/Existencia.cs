using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("Existencias")]
public class Existencia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdExistencia { get; set; }

    // Referencia logica a Catalogo.Api (sin constraint en base de datos)
    [Required(ErrorMessage = "El IdProducto es obligatorio.")]
    public int IdProducto { get; set; }

    // Relacion fisica con la tabla Bodegas
    [Required(ErrorMessage = "El IdBodega es obligatorio.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
