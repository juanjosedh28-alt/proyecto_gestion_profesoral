using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

/// <summary>Respuesta de la API para <c>area_conocimiento</c> (lo que se pinta en pantalla).</summary>
public class AreaConocimientoVista
{
    public string Id { get; set; } = string.Empty;
    public string GranArea { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
}

/// <summary>
/// (v2) Datos del formulario de <c>area_conocimiento</c>. Mismas reglas que el DTO de la API
/// (validación en cliente y servidor). El código es el oficial del catálogo, por ejemplo 1A01.
/// </summary>
public class AreaConocimientoFormulario
{
    [Display(Name = "Código")]
    [Required(ErrorMessage = "El código es obligatorio.")]
    [StringLength(6, ErrorMessage = "El código no puede superar 6 caracteres.")]
    [RegularExpression(@"^[A-Za-z0-9]+$", ErrorMessage = "El código solo admite letras y números (por ejemplo 1A01).")]
    public string Id { get; set; } = string.Empty;

    [Display(Name = "Gran área")]
    [Required(ErrorMessage = "La gran área es obligatoria.")]
    [StringLength(60, ErrorMessage = "La gran área no puede superar 60 caracteres.")]
    public string GranArea { get; set; } = string.Empty;

    [Display(Name = "Área")]
    [Required(ErrorMessage = "El área es obligatoria.")]
    [StringLength(60, ErrorMessage = "El área no puede superar 60 caracteres.")]
    public string Area { get; set; } = string.Empty;

    [Display(Name = "Disciplina")]
    [Required(ErrorMessage = "La disciplina es obligatoria.")]
    [StringLength(150, ErrorMessage = "La disciplina no puede superar 150 caracteres.")]
    public string Disciplina { get; set; } = string.Empty;
}
