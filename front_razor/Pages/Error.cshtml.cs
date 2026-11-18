using front_razor.Infraestructura;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages;

/// <summary>
/// (v3) Página de error. La muestra UseStatusCodePagesWithReExecute cuando una
/// respuesta sale con 403 o 404 sin cuerpo, CONSERVANDO el código.
///
/// El caso importante es el 403: alguien escribió a mano la dirección de una
/// pantalla que su rol no puede abrir; la API respondió 403 y aquí se le dice,
/// sin mostrar ningún dato (criterio 9).
///
/// También la muestra UseExceptionHandler (código 500) cuando un error no lo
/// atendió ninguna página; si fue la API apagada, lo dice con esas palabras.
///
/// Tiene OnPost además de OnGet porque la nueva ejecución conserva el método:
/// si el 403 vino de un formulario (por ejemplo, Consulta intentando guardar),
/// esta página se ejecuta como POST. [IgnoreAntiforgeryToken]: aquí no se
/// guarda nada, solo se muestra el aviso.
/// </summary>
[IgnoreAntiforgeryToken]
public class ErrorModel : PageModel
{
    // El código se lee de la RUTA (/Error/403) y no de un parámetro del método:
    // así sirve aunque la nueva ejecución traiga otro handler en la dirección.
    public int Codigo => int.TryParse(RouteData.Values["codigo"]?.ToString(), out var c) ? c : 500;

    // Si se llegó aquí por una excepción (UseExceptionHandler), cuál fue.
    private Exception? Excepcion => HttpContext.Features.Get<IExceptionHandlerFeature>()?.Error;

    public string Titulo => Codigo switch
    {
        403 => "No tiene permiso",
        404 => "No encontrada",
        _ when Excepcion is ErrorApiException { Codigo: 503 } => "Sin conexión con la API",
        _ => "Algo salió mal"
    };

    public string Texto => Codigo switch
    {
        403 => "Su rol no puede abrir esta pantalla ni hacer esta operación. Si cree que debería, hable con un administrador.",
        404 => "La página que busca no existe.",
        _ when Excepcion is ErrorApiException api => api.Message,
        _ => "Ocurrió un error inesperado. Intente de nuevo."
    };

    public void OnGet() { }

    public void OnPost() { }
}
