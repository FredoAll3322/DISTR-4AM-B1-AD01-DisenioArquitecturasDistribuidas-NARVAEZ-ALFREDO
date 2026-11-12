using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Inventario.Api.Models;

[Table("Existencias")]
public class Existencia
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdExistencia { get; set; }

    /// <summary>
    /// LLAVE FORÁNEA LÓGICA: Referencia al IdProducto de Catalogo.Api (CatalogoDB).
    /// No tiene restricción física (FOREIGN KEY constraint) en la base de datos SQL para mantener el desacoplamiento de microservicios.
    /// </summary>
    [Required(ErrorMessage = "El IdProducto (referencia lógica a Catalogo) es obligatorio.")]
    public int IdProducto { get; set; }

    /// <summary>
    /// LLAVE FORÁNEA FÍSICA: Referencia interna a la tabla Bodegas dentro de InventarioDB.
    /// </summary>
    [Required(ErrorMessage = "El IdBodega es obligatorio.")]
    public int IdBodega { get; set; }

    [Required(ErrorMessage = "El stock es obligatorio.")]
    [Range(0, 1000000, ErrorMessage = "El stock no puede ser negativo.")]
    public int Stock { get; set; }

    [ForeignKey(nameof(IdBodega))]
    public Bodega? Bodega { get; set; }
}
