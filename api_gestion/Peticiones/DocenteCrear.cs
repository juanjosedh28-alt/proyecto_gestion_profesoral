using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// Cuerpo de las peticiones POST y PUT de <c>docente</c>.
/// Las anotaciones producen el 422 con mensajes en castellano.
/// </summary>
public class DocenteCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "La cédula debe ser mayor que 0.")]
    public int Cedula { get; set; }
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(60, ErrorMessage = "Los nombres no pueden superar 60 caracteres.")]
    public string Nombres { get; set; } = string.Empty;
    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(60, ErrorMessage = "Los apellidos no pueden superar 60 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;
    [Required(ErrorMessage = "El género es obligatorio.")]
    [StringLength(12, ErrorMessage = "El género no puede superar 12 caracteres.")]
    public string Genero { get; set; } = string.Empty;
    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(30, ErrorMessage = "El cargo no puede superar 30 caracteres.")]
    public string Cargo { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de nacimiento debe tener el formato aaaa-mm-dd.")]
    public string FechaNacimiento { get; set; } = string.Empty;
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(70, ErrorMessage = "El correo no puede superar 70 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato de correo válido.")]
    public string Correo { get; set; } = string.Empty;
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    [RegularExpression(@"^[0-9 +\-]+$", ErrorMessage = "El teléfono solo admite dígitos, espacios, + y -.")]
    public string Telefono { get; set; } = string.Empty;
    [Required(ErrorMessage = "La URL del CvLAC es obligatoria.")]
    [StringLength(128, ErrorMessage = "La URL del CvLAC no puede superar 128 caracteres.")]
    [RegularExpression(@"^https?://\S+$", ErrorMessage = "La URL del CvLAC debe comenzar por http:// o https://.")]
    public string UrlCvlac { get; set; } = string.Empty;
    [Required(ErrorMessage = "El escalafón es obligatorio.")]
    [StringLength(45, ErrorMessage = "El escalafón no puede superar 45 caracteres.")]
    public string Escalafon { get; set; } = string.Empty;
    [Required(ErrorMessage = "El perfil es obligatorio.")]
    [StringLength(4000, ErrorMessage = "El perfil no puede superar 4000 caracteres.")]
    public string Perfil { get; set; } = string.Empty;
    [StringLength(45, ErrorMessage = "La categoría Minciencias no puede superar 45 caracteres.")]
    public string? CatMinciencia { get; set; }
    [Required(ErrorMessage = "La convocatoria Minciencias es obligatoria.")]
    [StringLength(45, ErrorMessage = "La convocatoria Minciencias no puede superar 45 caracteres.")]
    public string ConvMinciencia { get; set; } = string.Empty;
    [Required(ErrorMessage = "La nacionalidad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La nacionalidad no puede superar 45 caracteres.")]
    public string Nacionalidaad { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "La línea de investigación principal debe ser mayor que 0.")]
    public int? LineaInvestigacionPrincipal { get; set; }
}
