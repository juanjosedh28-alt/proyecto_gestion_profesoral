using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

/// <summary>
/// (v3) POST /api/sesion — iniciar sesión.
///
/// [AllowAnonymous]: es el ÚNICO endpoint del módulo que no pide token,
/// porque quien está entrando todavía no tiene uno.
///
/// Respuestas:
///   200 → { token, email, nombre_completo, roles, expira }
///   401 → el MISMO mensaje si falló el correo o la contraseña: no se le dice
///         a un desconocido cuál de los dos está mal.
///   422 → faltó el correo o la contraseña (formulario incompleto).
/// </summary>
[ApiController]
[Route("api/sesion")]
[AllowAnonymous]
public class SesionController : ControllerBase
{
    private readonly IServicioSesion _servicio;

    public SesionController(IServicioSesion servicio) => _servicio = servicio;

    [HttpPost]
    public async Task<ActionResult<Sesion>> Iniciar([FromBody] InicioSesion peticion)
    {
        var sesion = await _servicio.IniciarAsync(peticion.Email, peticion.Contrasena);
        if (sesion is null)
            return Unauthorized(new { mensaje = "El correo o la contraseña no son correctos.", detalle = Array.Empty<string>() });

        return Ok(sesion);
    }
}
