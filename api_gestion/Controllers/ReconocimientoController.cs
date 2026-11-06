using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>reconocimiento</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/reconocimiento")]
public class ReconocimientoController : ControllerBase
{
    private readonly IServicioReconocimiento _servicio;

    public ReconocimientoController(IServicioReconocimiento servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Reconocimiento>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Reconocimiento>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<Reconocimiento>> Crear([FromBody] ReconocimientoCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/reconocimiento/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Reconocimiento>> Reemplazar(int id, [FromBody] ReconocimientoCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (docente): el detalle se lista desde su padre ----

    [HttpGet("/api/docente/{cedula:int}/reconocimiento")]
    public async Task<ActionResult<List<Reconocimiento>>> ListarPorDocente(int cedula) =>
        Ok(await _servicio.ListarPorDocenteAsync(cedula));

    // Varios renglones en UN SOLO envío (una petición, una transacción).
    [HttpPost("/api/docente/{cedula:int}/reconocimiento")]
    public async Task<ActionResult<List<Reconocimiento>>> CrearVarios(int cedula, [FromBody] List<ReconocimientoRenglon> peticion)
    {
        var creados = await _servicio.CrearLoteAsync(cedula, peticion);
        return Created($"/api/docente/{cedula}/reconocimiento", creados);
    }
}
