namespace ApiGestion.Modelos;

/// <summary>Entidad que representa una fila de la tabla <c>termino_clave</c>. También es el cuerpo de respuesta de la API (no incluye la columna <c>activo</c>).</summary>
public class TerminoClave
{
    public string Termino { get; set; } = string.Empty;
    public string? TerminoIngles { get; set; }
}
