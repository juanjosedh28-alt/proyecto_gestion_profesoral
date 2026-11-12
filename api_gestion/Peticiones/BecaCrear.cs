using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Cuerpo de las peticiones POST y PUT de <c>beca</c>.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class BecaCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El estudio debe ser mayor que 0.")]
    public int Estudios { get; set; }
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(80, ErrorMessage = "La institución no puede superar 80 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de inicio es obligatoria.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de inicio debe tener el formato aaaa-mm-dd.")]
    public string FechaInicio { get; set; } = string.Empty;
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de fin debe tener el formato aaaa-mm-dd.")]
    public string? FechaFin { get; set; }
}
