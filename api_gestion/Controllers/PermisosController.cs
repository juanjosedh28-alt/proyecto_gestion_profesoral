using ApiGestion.Repositorios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

/// <summary>
/// (v3) GET /api/permisos/mios — las rutas del usuario que pregunta, para que
/// el front ARME EL MENÚ.
///
/// - [Authorize]: exige un token válido (si no, 401). No exige una ruta: todo
///   usuario con sesión puede saber qué puede abrir.
/// - El correo se saca DEL TOKEN (User.Identity.Name), no de la petición: así
///   nadie puede preguntar por los permisos de otro.
/// - Esta lista NO protege nada: el menú es comodidad. La protección es
///   [ExigePermiso] en cada controlador.
/// </summary>
[ApiController]
[Route("api/permisos")]
[Authorize]
public class PermisosController : ControllerBase
{
    private readonly IRepositorioAcceso _acceso;

    public PermisosController(IRepositorioAcceso acceso) => _acceso = acceso;

    [HttpGet("mios")]
    public async Task<IActionResult> Mios()
    {
        var email = User.Identity?.Name;
        if (string.IsNullOrWhiteSpace(email))
            return Unauthorized(new { mensaje = "Debe iniciar sesión.", detalle = Array.Empty<string>() });

        var rutas = await _acceso.RutasPermitidasAsync(email);
        var roles = await _acceso.RolesAsync(email);
        return Ok(new { email, roles, rutas });
    }
}
