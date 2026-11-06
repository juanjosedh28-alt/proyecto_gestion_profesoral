namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>red_docente</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_red_docente</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class RedDocente
{
    public int Red { get; set; }
    public string RedNombre { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
    public string ActDestacadas { get; set; } = string.Empty;
}
