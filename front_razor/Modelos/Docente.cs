using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

/// <summary>Respuesta de la API para <c>docente</c> (lo que se pinta en pantalla).</summary>
public class DocenteVista
{
    public int Cedula { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string FechaNacimiento { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string UrlCvlac { get; set; } = string.Empty;
    public string FechaActualizacion { get; set; } = string.Empty;
    public string Escalafon { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string? CatMinciencia { get; set; }
    public string ConvMinciencia { get; set; } = string.Empty;
    public string Nacionalidaad { get; set; } = string.Empty;
    public int? LineaInvestigacionPrincipal { get; set; }
    public string? LineaInvestigacionPrincipalNombre { get; set; }
    /// <summary>Lo calcula el disparador trg_contar_estudios; el front nunca lo envía.</summary>
    public int TotalEstudios { get; set; }

    /// <summary>Texto que se muestra en los desplegables y títulos.</summary>
    public string NombreCompleto => $"{Nombres} {Apellidos}";
}

/// <summary>
/// Datos del formulario de <c>docente</c>. Mismas reglas que el DTO de la API.
/// No lleva fecha_actualizacion ni total_estudios: los pone la base de datos.
/// </summary>
public class DocenteFormulario
{
    [Display(Name = "Cédula")]
    [Required(ErrorMessage = "La cédula es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La cédula debe ser mayor que 0.")]
    public int? Cedula { get; set; }

    [Display(Name = "Nombres")]
    [Required(ErrorMessage = "Los nombres son obligatorios.")]
    [StringLength(60, ErrorMessage = "Los nombres no pueden superar 60 caracteres.")]
    public string Nombres { get; set; } = string.Empty;

    [Display(Name = "Apellidos")]
    [Required(ErrorMessage = "Los apellidos son obligatorios.")]
    [StringLength(60, ErrorMessage = "Los apellidos no pueden superar 60 caracteres.")]
    public string Apellidos { get; set; } = string.Empty;

    [Display(Name = "Género")]
    [Required(ErrorMessage = "El género es obligatorio.")]
    [StringLength(12, ErrorMessage = "El género no puede superar 12 caracteres.")]
    public string Genero { get; set; } = string.Empty;

    [Display(Name = "Cargo")]
    [Required(ErrorMessage = "El cargo es obligatorio.")]
    [StringLength(30, ErrorMessage = "El cargo no puede superar 30 caracteres.")]
    public string Cargo { get; set; } = string.Empty;

    [Display(Name = "Fecha de nacimiento")]
    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria.")]
    public string FechaNacimiento { get; set; } = string.Empty;

    [Display(Name = "Correo")]
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(70, ErrorMessage = "El correo no puede superar 70 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Correo { get; set; } = string.Empty;

    [Display(Name = "Teléfono")]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [StringLength(20, ErrorMessage = "El teléfono no puede superar 20 caracteres.")]
    public string Telefono { get; set; } = string.Empty;

    [Display(Name = "URL del CvLAC")]
    [Required(ErrorMessage = "La URL del CvLAC es obligatoria.")]
    [StringLength(128, ErrorMessage = "La URL del CvLAC no puede superar 128 caracteres.")]
    [RegularExpression(@"^https?://\S+$", ErrorMessage = "La URL debe comenzar por http:// o https://.")]
    public string UrlCvlac { get; set; } = string.Empty;

    [Display(Name = "Escalafón")]
    [Required(ErrorMessage = "El escalafón es obligatorio.")]
    [StringLength(45, ErrorMessage = "El escalafón no puede superar 45 caracteres.")]
    public string Escalafon { get; set; } = string.Empty;

    [Display(Name = "Perfil")]
    [Required(ErrorMessage = "El perfil es obligatorio.")]
    [StringLength(4000, ErrorMessage = "El perfil no puede superar 4000 caracteres.")]
    public string Perfil { get; set; } = string.Empty;

    [Display(Name = "Categoría Minciencias (opcional)")]
    [StringLength(45, ErrorMessage = "La categoría no puede superar 45 caracteres.")]
    public string? CatMinciencia { get; set; }

    [Display(Name = "Convocatoria Minciencias")]
    [Required(ErrorMessage = "La convocatoria es obligatoria.")]
    [StringLength(45, ErrorMessage = "La convocatoria no puede superar 45 caracteres.")]
    public string ConvMinciencia { get; set; } = string.Empty;

    [Display(Name = "Nacionalidad")]
    [Required(ErrorMessage = "La nacionalidad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La nacionalidad no puede superar 45 caracteres.")]
    public string Nacionalidaad { get; set; } = string.Empty;

    /// <summary>Clave foránea OPCIONAL: int? para que «(ninguna)» viaje como null y no como texto vacío.</summary>
    [Display(Name = "Línea de investigación principal")]
    public int? LineaInvestigacionPrincipal { get; set; }

    public static DocenteFormulario Desde(DocenteVista v) => new()
    {
        Cedula = v.Cedula, Nombres = v.Nombres, Apellidos = v.Apellidos, Genero = v.Genero, Cargo = v.Cargo,
        FechaNacimiento = v.FechaNacimiento, Correo = v.Correo, Telefono = v.Telefono, UrlCvlac = v.UrlCvlac,
        Escalafon = v.Escalafon, Perfil = v.Perfil, CatMinciencia = v.CatMinciencia, ConvMinciencia = v.ConvMinciencia,
        Nacionalidaad = v.Nacionalidaad, LineaInvestigacionPrincipal = v.LineaInvestigacionPrincipal
    };
}
