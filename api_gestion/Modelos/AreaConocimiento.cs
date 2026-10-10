namespace ApiGestion.Modelos;

/// <summary>Entidad que representa una fila de la tabla <c>area_conocimiento</c>. También es el cuerpo de respuesta de la API (no incluye la columna <c>activo</c>).</summary>
public class AreaConocimiento
{
    public string Id { get; set; } = string.Empty;
    public string GranArea { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string Disciplina { get; set; } = string.Empty;
}
