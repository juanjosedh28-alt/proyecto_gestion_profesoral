namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>estudio_ac</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_estudio_ac</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class EstudioAc
{
    public int Estudio { get; set; }
    public string EstudioTitulo { get; set; } = string.Empty;
    public string AreaConocimiento { get; set; } = string.Empty;
    public string AreaConocimientoDisciplina { get; set; } = string.Empty;
}
