using System.Text.Json;

namespace front_razor.Infraestructura;

/// <summary>
/// (v3) Lo que el front recuerda de la persona que inició sesión, guardado en
/// la SESIÓN del servidor (ISession): el token, el correo, el nombre, los roles
/// y las rutas que puede abrir.
///
/// POR QUÉ EN LA SESIÓN DEL SERVIDOR: este front arma el HTML en el servidor
/// (Razor Pages). El navegador solo guarda una cookie con el id de la sesión
/// (HttpOnly: un script de la página no la puede leer); el token nunca llega
/// al navegador.
///
/// Las RUTAS solo sirven para armar el menú y esconder botones: eso es
/// comodidad. Quien protege es la API, que comprueba el permiso en cada
/// petición.
/// </summary>
public static class SesionUsuario
{
    private const string ClaveToken = "token";
    private const string ClaveEmail = "email";
    private const string ClaveNombre = "nombre";
    private const string ClaveRoles = "roles";
    private const string ClaveRutas = "rutas";

    public static void Guardar(ISession sesion, string token, string email, string nombre, IEnumerable<string> roles)
    {
        sesion.SetString(ClaveToken, token);
        sesion.SetString(ClaveEmail, email);
        sesion.SetString(ClaveNombre, nombre);
        sesion.SetString(ClaveRoles, string.Join(", ", roles));
    }

    public static void GuardarRutas(ISession sesion, IEnumerable<string> rutas) =>
        sesion.SetString(ClaveRutas, JsonSerializer.Serialize(rutas));

    public static void Cerrar(ISession sesion) => sesion.Clear();

    public static string? Token(ISession sesion) => sesion.GetString(ClaveToken);

    public static bool Iniciada(ISession sesion) => !string.IsNullOrEmpty(Token(sesion));

    public static string Nombre(ISession sesion) => sesion.GetString(ClaveNombre) ?? string.Empty;

    public static string Email(ISession sesion) => sesion.GetString(ClaveEmail) ?? string.Empty;

    public static string Roles(ISession sesion) => sesion.GetString(ClaveRoles) ?? string.Empty;

    public static List<string> Rutas(ISession sesion)
    {
        var texto = sesion.GetString(ClaveRutas);
        return string.IsNullOrEmpty(texto) ? new() : JsonSerializer.Deserialize<List<string>>(texto) ?? new();
    }

    /// <summary>¿Puede abrir esa ruta? Solo para el menú y los botones; la API decide de verdad.</summary>
    public static bool Tiene(ISession sesion, string ruta) => Rutas(sesion).Contains(ruta);
}
