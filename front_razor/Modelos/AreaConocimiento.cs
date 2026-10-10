namespace front_razor.Modelos;

/// <summary>Respuesta de la API para <c>area_conocimiento</c> (lo que se pinta en pantalla).</summary>
public class AreaConocimientoVista
{
    public string Id { get; set; } = string.Empty;
    public string GranArea { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
}
