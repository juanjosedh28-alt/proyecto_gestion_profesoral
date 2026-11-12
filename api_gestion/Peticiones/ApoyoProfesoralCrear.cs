using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Cuerpo de las peticiones POST y PUT de <c>apoyo_profesoral</c>.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class ApoyoProfesoralCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El estudio debe ser mayor que 0.")]
    public int Estudios { get; set; }
    [Range(0, 1, ErrorMessage = "Con apoyo debe ser 0 (no) o 1 (sí).")]
    public int ConApoyo { get; set; }
    [Required(ErrorMessage = "La institución es obligatoria.")]
    [StringLength(45, ErrorMessage = "La institución no puede superar 45 caracteres.")]
    public string Institucion { get; set; } = string.Empty;
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
}
