namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>evaluacion_docente</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_evaluacion_docente</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class EvaluacionDocente
{
    public int Id { get; set; }
    public decimal Calificacion { get; set; }
    public string Semestre { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}
