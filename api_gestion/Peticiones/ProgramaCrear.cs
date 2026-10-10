using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

public class ProgramaCrear
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string Nombre { get; set; } = string.Empty;

    public string Codigo { get; set; } = string.Empty;
}
