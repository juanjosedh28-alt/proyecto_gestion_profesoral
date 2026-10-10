using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>Cuerpo de las peticiones POST y PUT de <c>termino_clave</c>. Las anotaciones producen el 422 con mensajes en español.</summary>
public class TerminoClaveCrear
{
    [StringLength(30, ErrorMessage = "El término no puede superar 30 caracteres.")]
    public string? Termino { get; set; }
    [StringLength(30, ErrorMessage = "El término en inglés no puede superar 30 caracteres.")]
    public string? TerminoIngles { get; set; }
}
