using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

public class ProgramaCrear
{
    [Required(ErrorMessage = "El ID es obligatorio")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "El Nombre es obligatorio")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El Tipo es obligatorio")]
    public string? Tipo { get; set; }

    [Required(ErrorMessage = "El Nivel es obligatorio")]
    public string? Nivel { get; set; }

    [Required(ErrorMessage = "La Fecha de Creación es obligatoria")]
    public string? FechaCreacion { get; set; }

    public string? FechaCierre { get; set; } // Opcional

    [Required(ErrorMessage = "El Número de Cohortes es obligatorio")]
    public string? NumeroCohortes { get; set; }

    [Required(ErrorMessage = "La Cantidad de Graduados es obligatoria")]
    public string? CantGraduados { get; set; }

    [Required(ErrorMessage = "La Fecha de Actualización es obligatoria")]
    public string? FechaActualizacion { get; set; }

    [Required(ErrorMessage = "La Ciudad es obligatoria")]
    public string? Ciudad { get; set; }

    [Required(ErrorMessage = "La Facultad es obligatoria")]
    public int? Facultad { get; set; }
}