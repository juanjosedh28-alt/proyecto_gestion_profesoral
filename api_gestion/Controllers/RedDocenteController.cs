using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>red_docente</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/red-docente")]
public class RedDocenteController : ControllerBase
{
    private readonly IServicioRedDocente _servicio;

    public RedDocenteController(IServicioRedDocente servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<RedDocente>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{red:int}/{docente:int}")]
    public async Task<ActionResult<RedDocente>> Obtener(int red, int docente) => Ok(await _servicio.ObtenerAsync(red, docente));

    [HttpPost]
    public async Task<ActionResult<RedDocente>> Crear([FromBody] RedDocenteCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/red-docente/{creado.Red}/{creado.Docente}", creado);
    }

    [HttpPut("{red:int}/{docente:int}")]
    public async Task<ActionResult<RedDocente>> Reemplazar(int red, int docente, [FromBody] RedDocenteCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(red, docente, peticion));

    [HttpDelete("{red:int}/{docente:int}")]
    public async Task<IActionResult> Eliminar(int red, int docente)
    {
        await _servicio.EliminarAsync(red, docente);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (docente): el detalle se lista desde su padre ----

    [HttpGet("/api/docente/{cedula:int}/red-docente")]
    public async Task<ActionResult<List<RedDocente>>> ListarPorDocente(int cedula) =>
        Ok(await _servicio.ListarPorDocenteAsync(cedula));
}
