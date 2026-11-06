using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>estudios_realizados</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/estudios-realizados")]
public class EstudiosRealizadosController : ControllerBase
{
    private readonly IServicioEstudiosRealizados _servicio;

    public EstudiosRealizadosController(IServicioEstudiosRealizados servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<EstudiosRealizados>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EstudiosRealizados>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<EstudiosRealizados>> Crear([FromBody] EstudiosRealizadosCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/estudios-realizados/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<EstudiosRealizados>> Reemplazar(int id, [FromBody] EstudiosRealizadosCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (docente): el detalle se lista desde su padre ----

    [HttpGet("/api/docente/{cedula:int}/estudios-realizados")]
    public async Task<ActionResult<List<EstudiosRealizados>>> ListarPorDocente(int cedula) =>
        Ok(await _servicio.ListarPorDocenteAsync(cedula));

    // Varios renglones en UN SOLO envío (una petición, una transacción).
    [HttpPost("/api/docente/{cedula:int}/estudios-realizados")]
    public async Task<ActionResult<List<EstudiosRealizados>>> CrearVarios(int cedula, [FromBody] List<EstudiosRealizadosRenglon> peticion)
    {
        var creados = await _servicio.CrearLoteAsync(cedula, peticion);
        return Created($"/api/docente/{cedula}/estudios-realizados", creados);
    }
}
