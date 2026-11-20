using front_razor.Clientes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace front_razor.Infraestructura;

/// <summary>
/// (v3) Filtro que corre antes de CADA página:
///
/// 1. Si no hay sesión, lleva a /Sesion/Iniciar (y después devuelve a la
///    página que se pidió). Las únicas páginas sin sesión son el inicio de
///    sesión y la de error.
/// 2. Si hay sesión, vuelve a pedir a la API las rutas permitidas
///    (GET /api/permisos/mios) en cada página que se abre. Así el MENÚ
///    también refleja al instante un permiso quitado o dado, sin volver a
///    iniciar sesión.
///
/// Esto es COMODIDAD: evita mostrar pantallas que no se van a poder usar.
/// La PROTECCIÓN está en la API: si alguien llega a una página sin permiso,
/// la API responde 403 y se muestra «No tiene permiso».
/// </summary>
public class FiltroSesion : IAsyncPageFilter
{
    private static readonly string[] PaginasLibres = { "/Sesion/Iniciar", "/Error" };

    private readonly ClienteSesion _cliente;

    public FiltroSesion(ClienteSesion cliente) => _cliente = cliente;

    public Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext contexto) => Task.CompletedTask;

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext contexto, PageHandlerExecutionDelegate siguiente)
    {
        var pagina = contexto.ActionDescriptor.ViewEnginePath;
        if (PaginasLibres.Contains(pagina))
        {
            await siguiente();
            return;
        }

        var sesion = contexto.HttpContext.Session;
        if (!SesionUsuario.Iniciada(sesion))
        {
            var volver = contexto.HttpContext.Request.Path + contexto.HttpContext.Request.QueryString;
            contexto.Result = new RedirectToPageResult("/Sesion/Iniciar", new { volver });
            return;
        }

        if (HttpMethods.IsGet(contexto.HttpContext.Request.Method))
        {
            try
            {
                var permisos = await _cliente.MisPermisosAsync();   // 401 aquí = sesión vencida → ManejadorAcceso
                SesionUsuario.GuardarRutas(sesion, permisos.Rutas);
            }
            catch (ErrorApiException)
            {
                // La API no contestó (apagada o con error): se deja el menú que ya había y la
                // página sigue; ella misma pide sus datos y muestra «No se pudo conectar con la API».
                // (La sesión vencida no cae aquí: SesionVencidaException no es ErrorApiException.)
            }
        }

        await siguiente();
    }
}
