namespace ApiGestion.Modelos;

/// <summary>Entidad que representa una fila de la tabla <c>red</c>. También es el cuerpo de respuesta de la API (no incluye la columna <c>activo</c>).</summary>
public class Red
{
    public int Idr { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Pais { get; set; } = string.Empty;
}
