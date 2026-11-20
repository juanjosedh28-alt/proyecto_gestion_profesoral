using ApiGestion.Repositorios;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ApiGestion.Autorizacion;

/// <summary>
/// (v3) EL 403: aquí es donde el permiso se hace valer.
///
/// Se pone sobre un controlador o sobre un método:
///
///     [ExigePermiso("interfaz.docentes")]   → para entrar a cualquier operación del controlador
///     [ExigePermiso("datos.modificar")]     → además, para crear, editar o retirar
///
/// QUÉ HACE: antes de que el método del controlador corra, le pregunta a la
/// base (sp_verificar_acceso_ruta) si el correo del token tiene esa ruta. Si
/// no, responde 403 y el método NO se ejecuta.
///
/// TRES DECISIONES:
/// 1. Se consulta EN CADA PETICIÓN, no al iniciar sesión. Por eso quitarle un
///    permiso a un rol surte efecto de inmediato, sin volver a iniciar sesión
///    (criterio 7). Si el permiso fuera dentro del token, la persona seguiría
///    entrando hasta que el token venciera.
/// 2. 403 y no 401: el token es válido y se sabe quién pregunta; lo que falta
///    es el permiso. 401 = «no sé quién es usted»; 403 = «sé quién es, y no puede».
/// 3. Es un ATRIBUTO, no una línea dentro de cada método: está a la vista sobre
///    cada controlador y no se olvida al escribir un método nuevo.
///
/// AllowMultiple = true: un método de escritura lleva el de la clase
/// (interfaz.x) y además el suyo (datos.modificar). Corren los dos.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class ExigePermisoAttribute : Attribute, IAsyncAuthorizationFilter
{
    public string Ruta { get; }

    public ExigePermisoAttribute(string ruta) => Ruta = ruta;

    public async Task OnAuthorizationAsync(AuthorizationFilterContext contexto)
    {
        // Quién pregunta: lo dejó el middleware de autenticación al validar el
        // token. Si no hay token válido, [Authorize] ya respondió 401 y aquí no
        // se llega; se comprueba igual, porque un filtro que supone puede un
        // día dejar pasar a alguien.
        var email = contexto.HttpContext.User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
        {
            contexto.Result = new ObjectResult(new { mensaje = "Debe iniciar sesión.", detalle = Array.Empty<string>() })
            { StatusCode = 401 };
            return;
        }

        var acceso = contexto.HttpContext.RequestServices.GetRequiredService<IRepositorioAcceso>();

        if (!await acceso.TieneAccesoAsync(email, Ruta))
        {
            // 403: se sabe quién es, y su rol no puede.
            contexto.Result = new ObjectResult(new
            {
                mensaje = "Su rol no tiene permiso para esta operación.",
                detalle = new[] { $"permiso requerido: {Ruta}" }
            })
            { StatusCode = 403 };
        }
        // Si tiene el permiso, no se hace nada: la petición sigue hacia el método.
    }
}
