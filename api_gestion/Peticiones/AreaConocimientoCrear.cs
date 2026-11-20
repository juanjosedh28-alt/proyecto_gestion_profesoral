using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// (v2) Cuerpo de las peticiones POST y PUT de <c>area_conocimiento</c>.
/// El id es el código oficial del catálogo (por ejemplo «1A01»): letras y números,
/// hasta 6. Las anotaciones producen el 422 con mensajes en español.
/// </summary>
public class AreaConocimientoCrear
{
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(6, ErrorMessage = "El código no puede superar 6 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "El código solo admite letras y números (por ejemplo 1A01).")]
    public string Id { get; set; } = string.Empty;
    [Required(ErrorMessage = "La gran área es obligatoria.")]
    [StringLength(60, ErrorMessage = "La gran área no puede superar 60 caracteres.")]
    public string GranArea { get; set; } = string.Empty;
    [Required(ErrorMessage = "El área es obligatoria.")]
    [StringLength(60, ErrorMessage = "El área no puede superar 60 caracteres.")]
    public string Area { get; set; } = string.Empty;
    [Required(ErrorMessage = "La disciplina es obligatoria.")]
    [StringLength(150, ErrorMessage = "La disciplina no puede superar 150 caracteres.")]
    public string Disciplina { get; set; } = string.Empty;
}
