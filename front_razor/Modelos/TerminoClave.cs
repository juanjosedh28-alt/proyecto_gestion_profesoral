using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

/// <summary>Respuesta de la API para <c>termino_clave</c> (lo que se pinta en pantalla).</summary>
public class TerminoClaveVista
{
    public string Termino { get; set; } = string.Empty;
    public string? TerminoIngles { get; set; }
}

/// <summary>Datos del formulario de <c>termino_clave</c>. Mismas reglas que el DTO de la API (validación en cliente y servidor).</summary>
public class TerminoClaveFormulario
{
    [Display(Name = "Término")]
    [Required(ErrorMessage = "El término es obligatorio.")]
    [StringLength(30, ErrorMessage = "El término no puede superar 30 caracteres.")]
    public string Termino { get; set; } = string.Empty;
    [Display(Name = "Término en inglés")]
    [StringLength(30, ErrorMessage = "El término en inglés no puede superar 30 caracteres.")]
    public string? TerminoIngles { get; set; }
}
