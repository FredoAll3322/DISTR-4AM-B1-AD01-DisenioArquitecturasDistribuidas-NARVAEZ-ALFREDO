using System.ComponentModel.DataAnnotations;

namespace Catalogo.Api.DTOs;

public class CategoriaCreateUpdateDto
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "La descripción no puede superar los 250 caracteres.")]
    public string? Descripcion { get; set; }
}

public class MarcaCreateUpdateDto
{
    [Required(ErrorMessage = "El nombre de la marca es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El país de origen es obligatorio.")]
    [StringLength(100, ErrorMessage = "El país de origen no puede superar los 100 caracteres.")]
    public string PaisOrigen { get; set; } = string.Empty;
}

public class ProductoCreateUpdateDto
{
    [Required(ErrorMessage = "El Id de categoría es obligatorio.")]
    public int IdCategoria { get; set; }

    [Required(ErrorMessage = "El Id de marca es obligatorio.")]
    public int IdMarca { get; set; }

    [Required(ErrorMessage = "El nombre del producto es obligatorio.")]
    [StringLength(150, ErrorMessage = "El nombre no puede superar los 150 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El SKU es obligatorio.")]
    [StringLength(50, ErrorMessage = "El SKU no puede superar los 50 caracteres.")]
    public string SKU { get; set; } = string.Empty;

    [Required(ErrorMessage = "El precio es obligatorio.")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe ser mayor a 0.")]
    public decimal Precio { get; set; }

    [Required(ErrorMessage = "El estado es obligatorio (ej: Activo, Inactivo).")]
    public string Estado { get; set; } = "Activo";
}
