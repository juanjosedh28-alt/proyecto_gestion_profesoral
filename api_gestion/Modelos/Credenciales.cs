namespace ApiGestion.Modelos;

/// <summary>
/// (v3) Lo que se necesita para comprobar una contraseña: el HASH guardado.
/// Este modelo NUNCA sale en una respuesta de la API: solo lo usa
/// ServicioSesion por dentro para comparar con bcrypt.
/// </summary>
public class Credenciales
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string Hash { get; set; } = string.Empty;
}
