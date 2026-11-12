namespace ApiGestion.Modelos;

/// <summary>
/// Una fila de <c>docente</c> tal como la devuelve la API.
/// Son las columnas de la vista <c>vw_docente</c> menos <c>activo</c>: las fechas
/// vienen como texto "aaaa-mm-dd" y, cuando hay un padre, también su nombre para
/// mostrarlo en pantalla sin otra consulta.
/// </summary>
public class Docente
{
    public int Cedula { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Genero { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public string FechaNacimiento { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string UrlCvlac { get; set; } = string.Empty;
    public string FechaActualizacion { get; set; } = string.Empty;
    public string Escalafon { get; set; } = string.Empty;
    public string Perfil { get; set; } = string.Empty;
    public string? CatMinciencia { get; set; }
    public string ConvMinciencia { get; set; } = string.Empty;
    public string Nacionalidaad { get; set; } = string.Empty;
    public int? LineaInvestigacionPrincipal { get; set; }
    public string? LineaInvestigacionPrincipalNombre { get; set; }
    public int TotalEstudios { get; set; }
}
