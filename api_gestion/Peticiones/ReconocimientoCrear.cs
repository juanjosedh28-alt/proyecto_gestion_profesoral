using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Un renglón de <c>reconocimiento</c> SIN el docente. Es lo que viaja en el envío
/// múltiple (POST /api/docente/{cedula}/reconocimiento): ahí el docente sale de la
/// ruta, así que no se repite en cada renglón.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class ReconocimientoRenglon
{
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha debe tener el formato aaaa-mm-dd.")]
    public string Fecha { get; set; } = string.Empty;
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(45, ErrorMessage = "La institución no puede superar 45 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre no puede superar 45 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El ámbito es obligatorio.")]
    [StringLength(45, ErrorMessage = "El ámbito no puede superar 45 caracteres.")]
    public string Ambito { get; set; } = string.Empty;
}

/// <summary>
/// Cuerpo de POST y PUT de <c>reconocimiento</c> cuando se envía un solo registro:
/// el renglón más el docente al que pertenece (clave foránea).
/// </summary>
public class ReconocimientoCrear : ReconocimientoRenglon
{
    [Range(1, int.MaxValue, ErrorMessage = "El docente debe ser mayor que 0.")]
    public int Docente { get; set; }
}
