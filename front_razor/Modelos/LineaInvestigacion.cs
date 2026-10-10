using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

/// <summary>Respuesta de la API para <c>linea_investigacion</c> (lo que se pinta en pantalla).</summary>
public class LineaInvestigacionVista
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>Datos del formulario de <c>linea_investigacion</c>. Mismas reglas que el DTO de la API (validación en cliente y servidor).</summary>
public class LineaInvestigacionFormulario
{
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nombre no puede superar 45 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Display(Name = "Descripción")]
    [Required(ErrorMessage = "La descripción es obligatoria.")]
    [StringLength(256, ErrorMessage = "La descripción no puede superar 256 caracteres.")]
    public string Descripcion { get; set; } = string.Empty;
}
