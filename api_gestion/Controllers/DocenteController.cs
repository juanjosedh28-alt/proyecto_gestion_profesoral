using ApiGestion.Autorizacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>docente</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/docente")]
[Authorize]
[ExigePermiso("interfaz.docentes")]
public class DocenteController : ControllerBase
{
    private readonly IServicioDocente _servicio;

    public DocenteController(IServicioDocente servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Docente>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{cedula:int}")]
    public async Task<ActionResult<Docente>> Obtener(int cedula) => Ok(await _servicio.ObtenerAsync(cedula));

    [ExigePermiso("datos.modificar")]
    [HttpPost]
    public async Task<ActionResult<Docente>> Crear([FromBody] DocenteCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/docente/{creado.Cedula}", creado);
    }

    [ExigePermiso("datos.modificar")]
    [HttpPut("{cedula:int}")]
    public async Task<ActionResult<Docente>> Reemplazar(int cedula, [FromBody] DocenteCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(cedula, peticion));

    [ExigePermiso("datos.modificar")]
    [HttpDelete("{cedula:int}")]
    public async Task<IActionResult> Eliminar(int cedula)
    {
        await _servicio.EliminarAsync(cedula);
        return NoContent();
    }
}
