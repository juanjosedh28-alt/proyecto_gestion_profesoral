namespace ApiGestion.Modelos;

/// <summary>
/// (v3) Lo que devuelve POST /api/sesion cuando las credenciales son correctas.
/// Los roles van para MOSTRARLOS; los permisos NO van aquí ni en el token: se
/// consultan en cada petición.
/// </summary>
public class Sesion
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime Expira { get; set; }
}
