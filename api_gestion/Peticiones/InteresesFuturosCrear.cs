using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Cuerpo de las peticiones POST de <c>intereses_futuros</c>.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class InteresesFuturosCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El docente debe ser mayor que 0.")]
    public int Docente { get; set; }
    [Required(ErrorMessage = "El término clave es obligatorio.")]
    [StringLength(30, ErrorMessage = "El término clave no puede superar 30 caracteres.")]
    public string TerminoClave { get; set; } = string.Empty;
}
