namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>experiecia</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_experiecia</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class Experiecia
{
    public int Id { get; set; }
    public string NombreCargo { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}
