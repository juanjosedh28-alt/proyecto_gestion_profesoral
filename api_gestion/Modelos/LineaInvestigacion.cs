namespace ApiGestion.Modelos;

/// <summary>Entidad que representa una fila de la tabla <c>linea_investigacion</c>. También es el cuerpo de respuesta de la API (no incluye la columna <c>activo</c>).</summary>
public class LineaInvestigacion
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}
