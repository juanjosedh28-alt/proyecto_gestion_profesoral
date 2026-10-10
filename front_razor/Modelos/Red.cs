using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

/// <summary>Respuesta de la API para <c>red</c> (lo que se pinta en pantalla).</summary>
public class RedVista
{
    public int Idr { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
}

/// <summary>Datos del formulario de <c>red</c>. Mismas reglas que el DTO de la API (validación en cliente y servidor).</summary>
public class RedFormulario
{
    [Display(Name = "Id")]
    [Required(ErrorMessage = "El id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id debe ser mayor que 0.")]
    public int? Idr { get; set; }
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre no puede superar 45 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Display(Name = "URL")]
    [Required(ErrorMessage = "La URL es obligatoria.")]
    [StringLength(200, ErrorMessage = "La URL no puede superar 200 caracteres.")]
    [RegularExpression(@"^https?://\S+$", ErrorMessage = "La URL debe comenzar por http:// o https://.")]
    public string Url { get; set; } = string.Empty;
    [Display(Name = "País")]
    [Required(ErrorMessage = "El país es obligatorio.")]
    [StringLength(45, ErrorMessage = "El país no puede superar 45 caracteres.")]
    public string Pais { get; set; } = string.Empty;
}
