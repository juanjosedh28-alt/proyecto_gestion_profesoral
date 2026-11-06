using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>estudio_ac</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// No tiene PUT: es un puente puro, la pareja existe o no existe (regla R6).
/// </summary>
[ApiController]
[Route("api/estudio-ac")]
public class EstudioAcController : ControllerBase
{
    private readonly IServicioEstudioAc _servicio;

    public EstudioAcController(IServicioEstudioAc servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<EstudioAc>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{estudio:int}/{areaConocimiento}")]
    public async Task<ActionResult<EstudioAc>> Obtener(int estudio, string areaConocimiento) => Ok(await _servicio.ObtenerAsync(estudio, areaConocimiento));

    [HttpPost]
    public async Task<ActionResult<EstudioAc>> Crear([FromBody] EstudioAcCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/estudio-ac/{creado.Estudio}/{Uri.EscapeDataString(creado.AreaConocimiento)}", creado);
    }

    [HttpDelete("{estudio:int}/{areaConocimiento}")]
    public async Task<IActionResult> Eliminar(int estudio, string areaConocimiento)
    {
        await _servicio.EliminarAsync(estudio, areaConocimiento);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (estudios-realizados): el detalle se lista desde su padre ----

    [HttpGet("/api/estudios-realizados/{id:int}/estudio-ac")]
    public async Task<ActionResult<List<EstudioAc>>> ListarPorEstudio(int id) =>
        Ok(await _servicio.ListarPorEstudioAsync(id));
}
