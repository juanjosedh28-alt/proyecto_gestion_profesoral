using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Cuerpo de las peticiones POST y PUT de <c>docente_departamento</c>.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// En PUT las dos claves deben coincidir con las de la ruta: una pareja no cambia de dueño, solo se editan sus atributos.
/// </summary>
public class DocenteDepartamentoCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El docente debe ser mayor que 0.")]
    public int Docente { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "El programa debe ser mayor que 0.")]
    public int Departamento { get; set; }
    [Required(ErrorMessage = "La dedicación es obligatoria.")]
    [StringLength(15, ErrorMessage = "La dedicación no puede superar 15 caracteres.")]
    public string Dedicacion { get; set; } = string.Empty;
    [Required(ErrorMessage = "La modalidad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La modalidad no puede superar 45 caracteres.")]
    public string Modalidad { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de ingreso es obligatoria.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de ingreso debe tener el formato aaaa-mm-dd.")]
    public string FechaIngreso { get; set; } = string.Empty;
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de salida debe tener el formato aaaa-mm-dd.")]
    public string? FechaSalida { get; set; }
}
