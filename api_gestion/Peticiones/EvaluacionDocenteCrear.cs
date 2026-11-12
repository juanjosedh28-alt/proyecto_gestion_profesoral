using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Un renglón de <c>evaluacion_docente</c> SIN el docente. Es lo que viaja en el envío
/// múltiple (POST /api/docente/{cedula}/evaluacion-docente): ahí el docente sale de la
/// ruta, así que no se repite en cada renglón.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class EvaluacionDocenteRenglon
{
    [Range(typeof(decimal), "0", "5", ErrorMessage = "La calificación debe estar entre 0 y 5.")]
    public decimal Calificacion { get; set; }
    [Required(ErrorMessage = "El semestre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El semestre no puede superar 45 caracteres.")]
    public string Semestre { get; set; } = string.Empty;
}

/// <summary>
/// Cuerpo de POST y PUT de <c>evaluacion_docente</c> cuando se envía un solo registro:
/// el renglón más el docente al que pertenece (clave foránea).
/// </summary>
public class EvaluacionDocenteCrear : EvaluacionDocenteRenglon
{
    [Range(1, int.MaxValue, ErrorMessage = "El docente debe ser mayor que 0.")]
    public int Docente { get; set; }
}
