using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>Endpoints HTTP de <c>red</c>. Solo traduce HTTP ↔ servicio; los errores los convierte el middleware.</summary>
[ApiController]
[Route("api/red")]
public class RedController : ControllerBase
{
    private readonly IServicioRed _servicio;

    public RedController(IServicioRed servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Red>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{idr:int}")]
    public async Task<ActionResult<Red>> Obtener(int idr) => Ok(await _servicio.ObtenerAsync(idr));

    [HttpPost]
    public async Task<ActionResult<Red>> Crear([FromBody] RedCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/red/{creado.Idr}", creado);
    }

    [HttpPut("{idr:int}")]
    public async Task<ActionResult<Red>> Reemplazar(int idr, [FromBody] RedCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(idr, peticion));

    [HttpDelete("{idr:int}")]
    public async Task<IActionResult> Eliminar(int idr)
    {
        await _servicio.EliminarAsync(idr);
        return NoContent();
    }
}
