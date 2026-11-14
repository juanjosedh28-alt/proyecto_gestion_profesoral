using System.Net.Http.Json;
using System.Text.Json;
using front_razor.Infraestructura;

namespace front_razor.Clientes;

/// <summary>
/// Base de los clientes de la API: JSON en snake_case y traducción de errores HTTP a ErrorApiException.
/// (v3) Además traduce las dos respuestas del control de acceso:
///   401 con token → SesionVencidaException (la sesión ya no sirve)
///   403           → AccesoDenegadoException (su rol no puede)
/// Las dos las atiende el middleware ManejadorAcceso, no cada página.
/// El token lo pone ManejadorToken en cada petición.
/// </summary>
public abstract class ClienteApiBase
{
    private static readonly JsonSerializerOptions Opciones = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _http;

    protected ClienteApiBase(HttpClient http) => _http = http;

    protected static string Escapar(object clave) => Uri.EscapeDataString(clave.ToString() ?? string.Empty);

    protected async Task<T> GetAsync<T>(string ruta)
    {
        var respuesta = await EnviarAsync(HttpMethod.Get, ruta, null);
        return (await respuesta.Content.ReadFromJsonAsync<T>(Opciones))!;
    }

    /// <summary>
    /// (v2) Igual que GetAsync, pero si la API responde 404 devuelve null en vez de lanzar error.
    /// Sirve para el detalle 1:1 del estudio: que un estudio no tenga beca es normal, no un error.
    /// </summary>
    protected async Task<T?> GetOpcionalAsync<T>(string ruta) where T : class
    {
        try { return await GetAsync<T>(ruta); }
        catch (ErrorApiException ex) when (ex.Codigo == 404) { return null; }
    }

    protected Task PostAsync(string ruta, object cuerpo) => EnviarAsync(HttpMethod.Post, ruta, cuerpo);

    /// <summary>(v3) POST que además lee la respuesta (lo usa el inicio de sesión para recibir el token).</summary>
    protected async Task<T> PostAsync<T>(string ruta, object cuerpo)
    {
        var respuesta = await EnviarAsync(HttpMethod.Post, ruta, cuerpo);
        return (await respuesta.Content.ReadFromJsonAsync<T>(Opciones))!;
    }

    protected Task PutAsync(string ruta, object cuerpo) => EnviarAsync(HttpMethod.Put, ruta, cuerpo);

    protected Task DeleteAsync(string ruta) => EnviarAsync(HttpMethod.Delete, ruta, null);

    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string ruta, object? cuerpo)
    {
        HttpResponseMessage respuesta;
        var peticion = new HttpRequestMessage(metodo, ruta);
        try
        {
            if (cuerpo != null) peticion.Content = JsonContent.Create(cuerpo, cuerpo.GetType(), null, Opciones);
            respuesta = await _http.SendAsync(peticion);
        }
        catch (Exception e) when (e is HttpRequestException or TaskCanceledException)
        {
            throw new ErrorApiException(503, "No se pudo conectar con la API de gestión.");
        }

        if (respuesta.IsSuccessStatusCode) return respuesta;

        RespuestaError? error = null;
        try { error = await respuesta.Content.ReadFromJsonAsync<RespuestaError>(Opciones); } catch { /* cuerpo no JSON */ }

        // (v3) 401 de una petición que SÍ llevaba token = la sesión venció o no es válida.
        // (Un 401 sin token es el de un inicio de sesión fallido: ese sigue como ErrorApiException.)
        if (respuesta.StatusCode == System.Net.HttpStatusCode.Unauthorized && peticion.Headers.Authorization is not null)
            throw new SesionVencidaException(error?.Mensaje ?? "La sesión venció.");
        // (v3) 403 = la API sabe quién es y su rol no tiene el permiso.
        if (respuesta.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new AccesoDenegadoException(error?.Mensaje ?? "Su rol no tiene permiso para esta operación.");
        throw new ErrorApiException((int)respuesta.StatusCode, error?.Mensaje is { Length: > 0 } m ? m : "Ocurrió un error inesperado. Intente de nuevo.", error?.Detalle);
    }
}
