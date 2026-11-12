using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>evaluacion_docente</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/evaluacion-docente")]
public class EvaluacionDocenteController : ControllerBase
{
    private readonly IServicioEvaluacionDocente _servicio;

    public EvaluacionDocenteController(IServicioEvaluacionDocente servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<EvaluacionDocente>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EvaluacionDocente>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<EvaluacionDocente>> Crear([FromBody] EvaluacionDocenteCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/evaluacion-docente/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EvaluacionDocente>> Reemplazar(int id, [FromBody] EvaluacionDocenteCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (docente): el detalle se lista desde su padre ----

    [HttpGet("/api/docente/{cedula:int}/evaluacion-docente")]
    public async Task<ActionResult<List<EvaluacionDocente>>> ListarPorDocente(int cedula) =>
        Ok(await _servicio.ListarPorDocenteAsync(cedula));

    // Varios renglones en UN SOLO envío (una petición, una transacción).
    [HttpPost("/api/docente/{cedula:int}/evaluacion-docente")]
    public async Task<ActionResult<List<EvaluacionDocente>>> CrearVarios(int cedula, [FromBody] List<EvaluacionDocenteRenglon> peticion)
    {
        var creados = await _servicio.CrearLoteAsync(cedula, peticion);
        return Created($"/api/docente/{cedula}/evaluacion-docente", creados);
    }
}
