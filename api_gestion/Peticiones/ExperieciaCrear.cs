using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Un renglón de <c>experiecia</c> SIN el docente. Es lo que viaja en el envío
/// múltiple (POST /api/docente/{cedula}/experiecia): ahí el docente sale de la
/// ruta, así que no se repite en cada renglón.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class ExperieciaRenglon
{
    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El cargo no puede superar 45 caracteres.")]
    public string NombreCargo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(45, ErrorMessage = "La institución no puede superar 45 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de inicio debe tener el formato aaaa-mm-dd.")]
    public string FechaInicio { get; set; } = string.Empty;
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de fin debe tener el formato aaaa-mm-dd.")]
    public string? FechaFin { get; set; }
}

/// <summary>
/// Cuerpo de POST y PUT de <c>experiecia</c> cuando se envía un solo registro:
/// el renglón más el docente al que pertenece (clave foránea).
/// </summary>
public class ExperieciaCrear : ExperieciaRenglon
{
    [Range(1, int.MaxValue, ErrorMessage = "El docente debe ser mayor que 0.")]
    public int Docente { get; set; }
}
