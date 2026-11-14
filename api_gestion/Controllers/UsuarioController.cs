using ApiGestion.Autorizacion;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

/// <summary>
/// (v3) /api/usuario — administración de usuarios. SOLO para quien tenga la
/// ruta interfaz.usuarios (en la semilla, el Administrador).
///
/// Ningún método tiene una línea de control de acceso: el control está en los
/// dos atributos de la clase, que corren ANTES del método:
///   [Authorize]                          ¿el token es válido?     si no → 401
///   [ExigePermiso("interfaz.usuarios")]  ¿su rol tiene la ruta?   si no → 403
/// </summary>
[ApiController]
[Route("api/usuario")]
[Authorize]
[ExigePermiso("interfaz.usuarios")]
public class UsuarioController : ControllerBase
{
    private readonly IServicioUsuario _servicio;

    public UsuarioController(IServicioUsuario servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Usuario>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Usuario>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<Usuario>> Crear([FromBody] UsuarioCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/usuario/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Usuario>> Reemplazar(int id, [FromBody] UsuarioCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        // Quien pide sale del token: así el servicio sabe si alguien se intenta retirar a sí mismo.
        await _servicio.EliminarAsync(id, User.Identity?.Name);
        return NoContent();
    }

    /// <summary>Los roles de un usuario (filas de rol_usuario).</summary>
    [HttpGet("{id:int}/roles")]
    public async Task<ActionResult<List<RolUsuario>>> Roles(int id) => Ok(await _servicio.RolesAsync(id));
}

/// <summary>
/// (v3) /api/rol-usuario — asignar o quitar UN rol a un usuario. Mismo permiso
/// que los usuarios. Quitar un rol borra la fila (la tabla no tiene «activo»)
/// y surte efecto en la siguiente petición de esa persona.
/// </summary>
[ApiController]
[Route("api/rol-usuario")]
[Authorize]
[ExigePermiso("interfaz.usuarios")]
public class RolUsuarioController : ControllerBase
{
    private readonly IServicioUsuario _servicio;

    public RolUsuarioController(IServicioUsuario servicio) => _servicio = servicio;

    [HttpPost]
    public async Task<ActionResult<RolUsuario>> Asignar([FromBody] RolUsuarioCrear peticion)
    {
        var creado = await _servicio.AsignarRolAsync(peticion);
        return Created($"/api/usuario/{creado.UsuarioId}/roles", creado);
    }

    [HttpDelete("{usuarioId:int}/{rolId:int}")]
    public async Task<IActionResult> Quitar(int usuarioId, int rolId)
    {
        await _servicio.QuitarRolAsync(usuarioId, rolId);
        return NoContent();
    }
}
