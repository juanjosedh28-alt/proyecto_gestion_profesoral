namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>reconocimiento</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_reconocimiento</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class Reconocimiento
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Fecha { get; set; } = string.Empty;
    public string Institucion { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Ambito { get; set; } = string.Empty;
    public int Docente { get; set; }
    public string DocenteNombre { get; set; } = string.Empty;
}
