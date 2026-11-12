namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>docente_departamento</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_docente_departamento</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class DocenteDepartamento
{
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
    public int Departamento { get; set; }
    public string DepartamentoNombre { get; set; } = string.Empty;
    public string Dedicacion { get; set; } = string.Empty;
    public string Modalidad { get; set; } = string.Empty;
    public string FechaIngreso { get; set; } = string.Empty;
    public string? FechaSalida { get; set; }
}
