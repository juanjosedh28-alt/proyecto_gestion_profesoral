using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>Cuerpo de las peticiones POST y PUT de <c>linea_investigacion</c>. Las anotaciones producen el 422 con mensajes en español.</summary>
public class LineaInvestigacionCrear
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre no puede superar 45 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(256, ErrorMessage = "La descripción no puede superar 256 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}
