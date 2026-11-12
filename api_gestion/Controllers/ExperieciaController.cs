using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>experiecia</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/experiecia")]
public class ExperieciaController : ControllerBase
{
    private readonly IServicioExperiecia _servicio;

    public ExperieciaController(IServicioExperiecia servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Experiecia>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Experiecia>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<Experiecia>> Crear([FromBody] ExperieciaCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/experiecia/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Experiecia>> Reemplazar(int id, [FromBody] ExperieciaCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (docente): el detalle se lista desde su padre ----

    [HttpGet("/api/docente/{cedula:int}/experiecia")]
    public async Task<ActionResult<List<Experiecia>>> ListarPorDocente(int cedula) =>
        Ok(await _servicio.ListarPorDocenteAsync(cedula));

    // Varios renglones en UN SOLO envío (una petición, una transacción).
    [HttpPost("/api/docente/{cedula:int}/experiecia")]
    public async Task<ActionResult<List<Experiecia>>> CrearVarios(int cedula, [FromBody] List<ExperieciaRenglon> peticion)
    {
        var creados = await _servicio.CrearLoteAsync(cedula, peticion);
        return Created($"/api/docente/{cedula}/experiecia", creados);
    }
}
