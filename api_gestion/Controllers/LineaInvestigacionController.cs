using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>Endpoints HTTP de <c>linea_investigacion</c>. Solo traduce HTTP ↔ servicio; los errores los convierte el middleware.</summary>
[ApiController]
[Route("api/linea-investigacion")]
public class LineaInvestigacionController : ControllerBase
{
    private readonly IServicioLineaInvestigacion _servicio;

    public LineaInvestigacionController(IServicioLineaInvestigacion servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<LineaInvestigacion>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<LineaInvestigacion>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<LineaInvestigacion>> Crear([FromBody] LineaInvestigacionCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/linea-investigacion/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<LineaInvestigacion>> Reemplazar(int id, [FromBody] LineaInvestigacionCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}
