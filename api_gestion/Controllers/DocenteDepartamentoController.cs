using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>docente_departamento</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/docente-departamento")]
public class DocenteDepartamentoController : ControllerBase
{
    private readonly IServicioDocenteDepartamento _servicio;

    public DocenteDepartamentoController(IServicioDocenteDepartamento servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<DocenteDepartamento>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{docente:int}/{departamento:int}")]
    public async Task<ActionResult<DocenteDepartamento>> Obtener(int docente, int departamento) => Ok(await _servicio.ObtenerAsync(docente, departamento));

    [HttpPost]
    public async Task<ActionResult<DocenteDepartamento>> Crear([FromBody] DocenteDepartamentoCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/docente-departamento/{creado.Docente}/{creado.Departamento}", creado);
    }

    [HttpPut("{docente:int}/{departamento:int}")]
    public async Task<ActionResult<DocenteDepartamento>> Reemplazar(int docente, int departamento, [FromBody] DocenteDepartamentoCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(docente, departamento, peticion));

    [HttpDelete("{docente:int}/{departamento:int}")]
    public async Task<IActionResult> Eliminar(int docente, int departamento)
    {
        await _servicio.EliminarAsync(docente, departamento);
        return NoContent();
    }

    // ---- Rutas desde el MAESTRO (docente): el detalle se lista desde su padre ----

    [HttpGet("/api/docente/{cedula:int}/docente-departamento")]
    public async Task<ActionResult<List<DocenteDepartamento>>> ListarPorDocente(int cedula) =>
        Ok(await _servicio.ListarPorDocenteAsync(cedula));
}
