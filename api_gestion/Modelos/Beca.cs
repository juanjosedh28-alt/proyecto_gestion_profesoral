namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>beca</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_beca</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class Beca
{
    public int Estudios { get; set; }
    public string EstudioTitulo { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public string FechaInicio { get; set; } = string.Empty;
    public string? FechaFin { get; set; }
}
