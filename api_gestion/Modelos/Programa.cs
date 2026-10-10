namespace ApiGestion.Modelos;

<<<<<<< HEAD
=======
/// <summary>Entidad que representa una fila de la tabla <c>programa</c>. También es el cuerpo de respuesta de la API (no incluye la columna <c>activo</c>).</summary>
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
public class Programa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
<<<<<<< HEAD
    public string Codigo { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
=======
    public string Tipo { get; set; } = string.Empty;
    public string Nivel { get; set; } = string.Empty;
    public string FechaCreacion { get; set; } = string.Empty;
    public string? FechaCierre { get; set; }
    public string NumeroCohortes { get; set; } = string.Empty;
    public string CantGraduados { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public int Facultad { get; set; }
    public string FechaActualizacion { get; set; } = string.Empty;
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
}
