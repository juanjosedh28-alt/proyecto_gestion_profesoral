using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>beca</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/beca")]
public class BecaController : ControllerBase
{
    private readonly IServicioBeca _servicio;

    public BecaController(IServicioBeca servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Beca>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{estudios:int}")]
    public async Task<ActionResult<Beca>> Obtener(int estudios) => Ok(await _servicio.ObtenerAsync(estudios));

    [HttpPost]
    public async Task<ActionResult<Beca>> Crear([FromBody] BecaCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/beca/{creado.Estudios}", creado);
    }

    [HttpPut("{estudios:int}")]
    public async Task<ActionResult<Beca>> Reemplazar(int estudios, [FromBody] BecaCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(estudios, peticion));

    [HttpDelete("{estudios:int}")]
    public async Task<IActionResult> Eliminar(int estudios)
    {
        await _servicio.EliminarAsync(estudios);
        return NoContent();
    }
}
