namespace ApiGestion.Modelos;

public class Programa
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Codigo { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
}
