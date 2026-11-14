using ApiGestion.Autorizacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>Endpoints HTTP de <c>termino_clave</c>. Solo traduce HTTP ↔ servicio; los errores los convierte el middleware.</summary>
[ApiController]
[Route("api/termino-clave")]
[Authorize]
[ExigePermiso("interfaz.terminos")]
public class TerminoClaveController : ControllerBase
{
    private readonly IServicioTerminoClave _servicio;

    public TerminoClaveController(IServicioTerminoClave servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<TerminoClave>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{termino}")]
    public async Task<ActionResult<TerminoClave>> Obtener(string termino) => Ok(await _servicio.ObtenerAsync(termino));

    [ExigePermiso("datos.modificar")]
    [HttpPost]
    public async Task<ActionResult<TerminoClave>> Crear([FromBody] TerminoClaveCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/termino-clave/{creado.Termino}", creado);
    }

    [ExigePermiso("datos.modificar")]
    [HttpPut("{termino}")]
    public async Task<ActionResult<TerminoClave>> Reemplazar(string termino, [FromBody] TerminoClaveCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(termino, peticion));

    [ExigePermiso("datos.modificar")]
    [HttpDelete("{termino}")]
    public async Task<IActionResult> Eliminar(string termino)
    {
        await _servicio.EliminarAsync(termino);
        return NoContent();
    }
}
