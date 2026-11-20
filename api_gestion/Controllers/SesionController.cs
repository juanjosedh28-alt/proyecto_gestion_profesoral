using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

/// <summary>
/// (v3) POST /api/sesion — iniciar sesión. También responde en POST /api/login:
/// la guía del profesor (FLUJO_CONTROL_DE_ACCESO.md) usa /api/sesion, y la
/// metodología y el documento del módulo dicen /api/login. Las dos rutas
/// llegan al MISMO método, así que no hay dos lógicas que mantener.
///
/// Cerrado por defecto, abierto por excepción: [Authorize] en la CLASE y
/// [AllowAnonymous] solo en el MÉTODO de entrar, el único endpoint del módulo
/// que no pide token (quien entra todavía no tiene uno). Al revés, con
/// [AllowAnonymous] en la clase, cualquier método que se agregue después
/// quedaría abierto aunque lleve [Authorize] (el aviso ASP0026).
///
/// Respuestas:
///   200 → { token, email, nombre_completo, roles, expira }
///   401 → el MISMO mensaje si falló el correo o la contraseña: no se le dice
///         a un desconocido cuál de los dos está mal.
///   422 → faltó el correo o la contraseña (formulario incompleto).
/// </summary>
[ApiController]
[Route("api/sesion")]
[Authorize]
public class SesionController : ControllerBase
{
    private readonly IServicioSesion _servicio;

    public SesionController(IServicioSesion servicio) => _servicio = servicio;

    [AllowAnonymous]           // la excepción: quien entra todavía no tiene token
    [HttpPost]
    [HttpPost("/api/login")]   // ruta absoluta: no se le antepone «api/sesion»
    public async Task<ActionResult<Sesion>> Iniciar([FromBody] InicioSesion peticion)
    {
        var sesion = await _servicio.IniciarAsync(peticion.Email, peticion.Contrasena);
        if (sesion is null)
            return Unauthorized(new { mensaje = "El correo o la contraseña no son correctos.", detalle = Array.Empty<string>() });

        return Ok(sesion);
    }
}
