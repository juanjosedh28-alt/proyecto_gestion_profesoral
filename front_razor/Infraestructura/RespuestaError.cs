namespace front_razor.Infraestructura;

/// <summary>Formato único de error de la API: { "mensaje": "...", "detalle": ["campo: texto"] }.</summary>
public class RespuestaError
{
    public string Mensaje { get; set; } = string.Empty;
    public List<string> Detalle { get; set; } = new();
}
