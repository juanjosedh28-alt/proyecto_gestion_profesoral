using ApiGestion.Excepciones;

namespace ApiGestion.Middleware;

/// <summary>Convierte las excepciones de negocio en respuestas JSON con el formato único { mensaje, detalle } (404, 409, 422, 500).</summary>
public class ManejadorErrores
{
    private readonly RequestDelegate _siguiente;
    private readonly ILogger<ManejadorErrores> _log;

    public ManejadorErrores(RequestDelegate siguiente, ILogger<ManejadorErrores> log)
    {
        _siguiente = siguiente;
        _log = log;
    }

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (NoEncontradoExcepcion e) { await Responder(contexto, 404, e.Message, Array.Empty<string>()); }
        catch (ConflictoExcepcion e) { await Responder(contexto, 409, e.Message, Array.Empty<string>()); }
        catch (ValidacionExcepcion e) { await Responder(contexto, 422, e.Message, e.Detalle); }
        catch (Exception e)
        {
            _log.LogError(e, "Error inesperado");
            await Responder(contexto, 500, "Ocurrió un error interno. Intente de nuevo.", Array.Empty<string>());
        }
    }

    private static Task Responder(HttpContext contexto, int codigo, string mensaje, string[] detalle)
    {
        contexto.Response.StatusCode = codigo;
        return contexto.Response.WriteAsJsonAsync(new { mensaje, detalle });
    }
}
