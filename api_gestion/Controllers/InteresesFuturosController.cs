using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>intereses_futuros</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// No tiene PUT: es un puente puro, la pareja existe o no existe (regla R6).
/// </summary>
[ApiController]
[Route("api/intereses-futuros")]
public class InteresesFuturosController : ControllerBase
{
    private readonly IServicioInteresesFuturos _servicio;

    public InteresesFuturosController(IServicioInteresesFuturos servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<InteresesFuturos>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{docente:int}/{terminoClave}")]
    public async Task<ActionResult<InteresesFuturos>> Obtener(int docente, string terminoClave) => Ok(await _servicio.ObtenerAsync(docente, terminoClave));

    [HttpPost]
    public async Task<ActionResult<InteresesFuturos>> Crear([FromBody] InteresesFuturosCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/intereses-futuros/{creado.Docente}/{Uri.EscapeDataString(creado.TerminoClave)}", creado);
    }

    [HttpDelete("{docente:int}/{terminoClave}")]
    public async Task<IActionResult> Eliminar(int docente, string terminoClave)
    {
        await _servicio.EliminarAsync(docente, terminoClave);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (docente): el detalle se lista desde su padre ----

    [HttpGet("/api/docente/{cedula:int}/intereses-futuros")]
    public async Task<ActionResult<List<InteresesFuturos>>> ListarPorDocente(int cedula) =>
        Ok(await _servicio.ListarPorDocenteAsync(cedula));
}
