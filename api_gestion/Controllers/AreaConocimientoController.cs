using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>area_conocimiento</c>. En la v1 eran solo de lectura;
/// desde la v2 tienen CRUD completo (D65). Solo traduce HTTP ↔ servicio; los
/// errores (404, 409, 422) los convierte el middleware.
/// </summary>
[ApiController]
[Route("api/area-conocimiento")]
public class AreaConocimientoController : ControllerBase
{
    private readonly IServicioAreaConocimiento _servicio;

    public AreaConocimientoController(IServicioAreaConocimiento servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<AreaConocimiento>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<AreaConocimiento>> Obtener(string id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<AreaConocimiento>> Crear([FromBody] AreaConocimientoCrear peticion)
    {
        var creada = await _servicio.CrearAsync(peticion);
        return Created($"/api/area-conocimiento/{creada.Id}", creada);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<AreaConocimiento>> Reemplazar(string id, [FromBody] AreaConocimientoCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(string id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}
