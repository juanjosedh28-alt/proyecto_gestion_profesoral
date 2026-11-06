using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Cuerpo de las peticiones POST y PUT de <c>red_docente</c>.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// En PUT las dos claves deben coincidir con las de la ruta: una pareja no cambia de dueño, solo se editan sus atributos.
/// </summary>
public class RedDocenteCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "La red debe ser mayor que 0.")]
    public int Red { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El docente debe ser mayor que 0.")]
    public int Docente { get; set; }
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de inicio debe tener el formato aaaa-mm-dd.")]
    public string FechaInicio { get; set; } = string.Empty;
    [StringLength(45, ErrorMessage = "La fecha de fin no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de fin debe tener el formato aaaa-mm-dd.")]
    public string? FechaFin { get; set; }
    [Required(ErrorMessage = "Las actividades destacadas son obligatorias.")]
    [StringLength(4000, ErrorMessage = "Las actividades destacadas no pueden superar 4000 caracteres.")]
    public string ActDestacadas { get; set; } = string.Empty;
}
