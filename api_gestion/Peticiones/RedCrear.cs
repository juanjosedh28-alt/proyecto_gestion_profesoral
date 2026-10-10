using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>Cuerpo de las peticiones POST y PUT de <c>red</c>. Las anotaciones producen el 422 con mensajes en español.</summary>
public class RedCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El id debe ser mayor que 0.")]
    public int Idr { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre no puede superar 45 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "La URL es obligatoria.")]
    [StringLength(200, ErrorMessage = "La URL no puede superar 200 caracteres.")]
    [RegularExpression(@"^https?://\S+$", ErrorMessage = "La URL debe comenzar por http:// o https://.")]
    public string Url { get; set; } = string.Empty;
    [Required(ErrorMessage = "El país es obligatorio.")]
    [StringLength(45, ErrorMessage = "El país no puede superar 45 caracteres.")]
    public string Pais { get; set; } = string.Empty;
}
