using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

/// <summary>Respuesta de la API para <c>programa</c> (lo que se pinta en pantalla).</summary>
public class ProgramaVista
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public string FechaCreacion { get; set; } = string.Empty;
    public string? FechaCierre { get; set; }
    public string NumeroCohortes { get; set; } = string.Empty;
    public string CantGraduados { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public int Facultad { get; set; }
    public string FechaActualizacion { get; set; } = string.Empty;
}

/// <summary>Datos del formulario de <c>programa</c>. Mismas reglas que el DTO de la API (validación en cliente y servidor).</summary>
public class ProgramaFormulario
{
    [Display(Name = "Id")]
    [Required(ErrorMessage = "El id es obligatorio.")]
    [Range(1, int.MaxValue, ErrorMessage = "El id debe ser mayor que 0.")]
    public int? Id { get; set; }
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Display(Name = "Tipo")]
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Display(Name = "Nivel")]
    [Required(ErrorMessage = "El nivel es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nivel no puede superar 45 caracteres.")]
    public string Nivel { get; set; } = string.Empty;
    [Display(Name = "Fecha de creación")]
    [Required(ErrorMessage = "La fecha de creación es obligatoria.")]
    [StringLength(45, ErrorMessage = "La fecha de creación no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de creación debe tener el formato aaaa-mm-dd.")]
    public string FechaCreacion { get; set; } = string.Empty;
    [Display(Name = "Fecha de cierre")]
    [StringLength(45, ErrorMessage = "La fecha de cierre no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de cierre debe tener el formato aaaa-mm-dd.")]
    public string? FechaCierre { get; set; }
    [Display(Name = "Número de cohortes")]
    [Required(ErrorMessage = "El número de cohortes es obligatorio.")]
    [StringLength(45, ErrorMessage = "El número de cohortes no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{1,45}$", ErrorMessage = "El número de cohortes debe ser numérico.")]
    public string NumeroCohortes { get; set; } = string.Empty;
    [Display(Name = "Cantidad de graduados")]
    [Required(ErrorMessage = "La cantidad de graduados es obligatoria.")]
    [StringLength(45, ErrorMessage = "La cantidad de graduados no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{1,45}$", ErrorMessage = "La cantidad de graduados debe ser numérica.")]
    public string CantGraduados { get; set; } = string.Empty;
    [Display(Name = "Ciudad")]
    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La ciudad no puede superar 45 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;
    [Display(Name = "Facultad")]
    [Required(ErrorMessage = "La facultad es obligatoria.")]
    [Range(1, int.MaxValue, ErrorMessage = "La facultad debe ser mayor que 0.")]
    public int? Facultad { get; set; }
}
