using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Cuerpo de las peticiones POST de <c>estudio_ac</c>.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class EstudioAcCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El estudio debe ser mayor que 0.")]
    public int Estudio { get; set; }
    [Required(ErrorMessage = "El área de conocimiento es obligatorio.")]
    [StringLength(6, ErrorMessage = "El área de conocimiento no puede superar 6 caracteres.")]
    public string AreaConocimiento { get; set; } = string.Empty;
}
