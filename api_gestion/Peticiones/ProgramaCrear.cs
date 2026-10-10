using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

<<<<<<< HEAD
public class ProgramaCrear
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;
=======
/// <summary>Cuerpo de las peticiones POST y PUT de <c>programa</c>. Las anotaciones producen el 422 con mensajes en español.</summary>
public class ProgramaCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El id debe ser mayor que 0.")]
    public int Id { get; set; }
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(60, ErrorMessage = "El nombre no puede superar 60 caracteres.")]
    public string Nombre { get; set; } = string.Empty;
    [Required(ErrorMessage = "El tipo es obligatorio.")]
    [StringLength(45, ErrorMessage = "El tipo no puede superar 45 caracteres.")]
    public string Tipo { get; set; } = string.Empty;
    [Required(ErrorMessage = "El nivel es obligatorio.")]
    [StringLength(45, ErrorMessage = "El nivel no puede superar 45 caracteres.")]
    public string Nivel { get; set; } = string.Empty;
    [Required(ErrorMessage = "La fecha de creación es obligatoria.")]
    [StringLength(45, ErrorMessage = "La fecha de creación no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de creación debe tener el formato aaaa-mm-dd.")]
    public string FechaCreacion { get; set; } = string.Empty;
    [StringLength(45, ErrorMessage = "La fecha de cierre no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{4}-\d{2}-\d{2}$", ErrorMessage = "La fecha de cierre debe tener el formato aaaa-mm-dd.")]
    public string? FechaCierre { get; set; }
    [Required(ErrorMessage = "El número de cohortes es obligatorio.")]
    [StringLength(45, ErrorMessage = "El número de cohortes no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{1,45}$", ErrorMessage = "El número de cohortes debe ser numérico.")]
    public string NumeroCohortes { get; set; } = string.Empty;
    [Required(ErrorMessage = "La cantidad de graduados es obligatoria.")]
    [StringLength(45, ErrorMessage = "La cantidad de graduados no puede superar 45 caracteres.")]
    [RegularExpression(@"^\d{1,45}$", ErrorMessage = "La cantidad de graduados debe ser numérica.")]
    public string CantGraduados { get; set; } = string.Empty;
    [Required(ErrorMessage = "La ciudad es obligatoria.")]
    [StringLength(45, ErrorMessage = "La ciudad no puede superar 45 caracteres.")]
    public string Ciudad { get; set; } = string.Empty;
    [Range(1, int.MaxValue, ErrorMessage = "La facultad debe ser mayor que 0.")]
    public int Facultad { get; set; }
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
}
