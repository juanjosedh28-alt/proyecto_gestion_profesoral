using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Un renglón de <c>estudios_realizados</c> SIN el docente. Es lo que viaja en el envío
/// múltiple (POST /api/docente/{cedula}/estudios-realizados): ahí el docente sale de la
/// ruta, así que no se repite en cada renglón.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class EstudiosRealizadosRenglon
{
    [Range(1, int.MaxValue, ErrorMessage = "El id debe ser mayor que 0.")]
    public int Id { get; set; }
    [Required(ErrorMessage = "El título es obligatorio.")]
    [StringLength(45, ErrorMessage = "El título no puede superar 45 caracteres.")]
    public string Titulo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La universidad es obligatoria.")]
    [StringLength(50, ErrorMessage = "La universidad no puede superar 50 caracteres.")]
    public string Universidad { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha es obligatoria.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha debe tener el formato aaaa-mm-dd.")]
    public string Fecha { get; set; } = string.Empty;
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La ciudad no puede superar 45 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;
    [Range(0, 1, ErrorMessage = "Institución acreditada debe ser 0 (no) o 1 (sí).")]
    public int InsAcreditada { get; set; }
    [Required(ErrorMessage = "La metodología es obligatoria.")]
    [StringLength(45, ErrorMessage = "La metodología no puede superar 45 caracteres.")]
    public string Metodologia { get; set; } = string.Empty;
    [Required(ErrorMessage = "El perfil del egresado es obligatorio.")]
    [StringLength(4000, ErrorMessage = "El perfil del egresado no puede superar 4000 caracteres.")]
    public string PerfilEgresado { get; set; } = string.Empty;
    [Required(ErrorMessage = "El país es obligatorio.")]
    [StringLength(45, ErrorMessage = "El país no puede superar 45 caracteres.")]
    public string Pais { get; set; } = string.Empty;
}

/// <summary>
/// Cuerpo de POST y PUT de <c>estudios_realizados</c> cuando se envía un solo registro:
/// el renglón más el docente al que pertenece (clave foránea).
/// </summary>
public class EstudiosRealizadosCrear : EstudiosRealizadosRenglon
{
    [Range(1, int.MaxValue, ErrorMessage = "El docente debe ser mayor que 0.")]
    public int Docente { get; set; }
}
