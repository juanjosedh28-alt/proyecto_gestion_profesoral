using System.Net.Http.Json;
using System.Text.Json;
using front_razor.Infraestructura;

namespace front_razor.Clientes;

/// <summary>Base de los clientes de la API: JSON en snake_case y traducción de errores HTTP a ErrorApiException.</summary>
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

    protected Task PostAsync(string ruta, object cuerpo) => EnviarAsync(HttpMethod.Post, ruta, cuerpo);

    protected Task PutAsync(string ruta, object cuerpo) => EnviarAsync(HttpMethod.Put, ruta, cuerpo);

    protected Task DeleteAsync(string ruta) => EnviarAsync(HttpMethod.Delete, ruta, null);

    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string ruta, object? cuerpo)
    {
        HttpResponseMessage respuesta;
        try
        {
            var peticion = new HttpRequestMessage(metodo, ruta);
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
        throw new ErrorApiException((int)respuesta.StatusCode, error?.Mensaje is { Length: > 0 } m ? m : $"Error inesperado ({(int)respuesta.StatusCode}).", error?.Detalle);
    }
}
