namespace front_razor.Infraestructura;

/// <summary>
/// (v3) Middleware que atiende las dos respuestas del control de acceso que
/// pueden ocurrir en CUALQUIER página:
///
/// - 401 (SesionVencidaException): la sesión ya no sirve. Se cierra y se
///   lleva a la persona a iniciar sesión, con un aviso.
/// - 403 (AccesoDenegadoException): la API dijo que su rol no puede. Se
///   responde 403 y UseStatusCodePagesWithReExecute muestra /Error/403
///   («No tiene permiso») con el menú normal, el aviso de la API y el
///   permiso que faltó; ningún dato.
///
/// Así, si alguien sin permiso escribe a mano /Usuarios, ve el 403 que le dio
/// la API, no los datos (criterio 9). El menú escondido no era la protección.
/// </summary>
public class ManejadorAcceso
{
    // Lo que dijo la API en el 403, para que /Error/403 lo muestre tal cual.
    public const string ClaveMensaje = "acceso.mensaje";
    public const string ClaveDetalle = "acceso.detalle";

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
        catch (AccesoDenegadoException ex)
        {
            contexto.Response.Clear();
            contexto.Response.StatusCode = StatusCodes.Status403Forbidden;
            contexto.Items[ClaveMensaje] = ex.Message;    // «Su rol no tiene permiso para esta operación.»
            contexto.Items[ClaveDetalle] = ex.Detalle;    // «permiso requerido: interfaz.usuarios»
        }
    }
}
