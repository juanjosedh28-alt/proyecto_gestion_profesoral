namespace front_razor.Infraestructura;

/// <summary>
/// (v3) Middleware que atiende las dos respuestas del control de acceso que
/// pueden ocurrir en CUALQUIER página:
///
/// - 401 (SesionVencidaException): la sesión ya no sirve. Se cierra y se
///   lleva a la persona a iniciar sesión, con un aviso.
/// - 403 (AccesoDenegadoException): la API dijo que su rol no puede. Se
///   responde 403 y UseStatusCodePagesWithReExecute muestra /Error/403
///   («No tiene permiso») con el menú normal.
///
/// Así, si alguien sin permiso escribe a mano /Usuarios, ve el 403 que le dio
/// la API, no los datos (criterio 9). El menú escondido no era la protección.
/// </summary>
public class ManejadorAcceso
{
    private readonly RequestDelegate _siguiente;

    public ManejadorAcceso(RequestDelegate siguiente) => _siguiente = siguiente;

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (SesionVencidaException)
        {
            SesionUsuario.Cerrar(contexto.Session);
            contexto.Response.Clear();
            contexto.Response.Redirect("/Sesion/Iniciar?motivo=vencida");
        }
        catch (AccesoDenegadoException)
        {
            contexto.Response.Clear();
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
        }
    }
}
