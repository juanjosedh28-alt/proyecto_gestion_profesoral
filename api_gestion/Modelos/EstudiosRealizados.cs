namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>estudios_realizados</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_estudios_realizados</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class EstudiosRealizados
{
    public int Id { get; set; }
    public string Titulo { get; set; } = string.Empty;
    public string Universidad { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public string Ciudad { get; set; } = string.Empty;
    public int InsAcreditada { get; set; }
    public string Metodologia { get; set; } = string.Empty;
    public string PerfilEgresado { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}
