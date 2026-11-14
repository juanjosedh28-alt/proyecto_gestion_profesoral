using ApiGestion.Autorizacion;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

/// <summary>(v3) /api/rol — administración de roles. Exige la ruta interfaz.roles.</summary>
[ApiController]
[Route("api/rol")]
[Authorize]
[ExigePermiso("interfaz.roles")]
public class RolController : ControllerBase
{
    private readonly IServicioRol _servicio;

    public RolController(IServicioRol servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Rol>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Rol>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<Rol>> Crear([FromBody] RolCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/rol/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Rol>> Reemplazar(int id, [FromBody] RolCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}

/// <summary>
/// (v3) /api/ruta y /api/ruta-rol — los PERMISOS de cada rol. Exige la ruta
/// interfaz.permisos. Aquí se demuestra el criterio 7: al quitar un permiso
/// (DELETE /api/ruta-rol/{rutaId}/{rolId}), el usuario con ese rol recibe 403
/// en su siguiente petición, sin volver a iniciar sesión.
/// </summary>
[ApiController]
[Authorize]
[ExigePermiso("interfaz.permisos")]
public class PermisoRolController : ControllerBase
{
    private readonly IRepositorioPermiso _repo;

    public PermisoRolController(IRepositorioPermiso repo) => _repo = repo;

    /// <summary>El catálogo de rutas protegibles.</summary>
    [HttpGet("api/ruta")]
    public async Task<ActionResult<List<Ruta>>> Rutas() => Ok(await _repo.RutasAsync());

    /// <summary>Todas las rutas, marcando cuáles tiene ese rol.</summary>
    [HttpGet("api/ruta-rol/{rolId:int}")]
    public async Task<ActionResult<List<PermisoDeRol>>> DeRol(int rolId) => Ok(await _repo.PermisosDeRolAsync(rolId));

    [HttpPost("api/ruta-rol")]
    public async Task<IActionResult> Dar([FromBody] RutaRolCrear peticion)
    {
        await _repo.DarAsync(peticion.RutaId, peticion.RolId);
        return Created($"/api/ruta-rol/{peticion.RolId}", null);
    }

    [HttpDelete("api/ruta-rol/{rutaId:int}/{rolId:int}")]
    public async Task<IActionResult> Quitar(int rutaId, int rolId)
    {
        await _repo.QuitarAsync(rutaId, rolId);
        return NoContent();
    }
}
