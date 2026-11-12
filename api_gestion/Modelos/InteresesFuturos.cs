namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>intereses_futuros</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_intereses_futuros</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class InteresesFuturos
{
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
    public string TerminoClave { get; set; } = string.Empty;
    public string? TerminoIngles { get; set; }
}
