using ApiGestion.Autorizacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>Endpoints HTTP de <c>programa</c>. Solo traduce HTTP ↔ servicio; los errores los convierte el middleware.</summary>
[ApiController]
[Route("api/programa")]
[Authorize]
[ExigePermiso("interfaz.programas")]
public class ProgramaController : ControllerBase
{
    private readonly IServicioPrograma _servicio;

    public ProgramaController(IServicioPrograma servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Programa>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Programa>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [ExigePermiso("datos.modificar")]
    [HttpPost]
    public async Task<ActionResult<Programa>> Crear([FromBody] ProgramaCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/programa/{creado.Id}", creado);
    }

    [ExigePermiso("datos.modificar")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<Programa>> Reemplazar(int id, [FromBody] ProgramaCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [ExigePermiso("datos.modificar")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
        return NoContent();
    }
}
