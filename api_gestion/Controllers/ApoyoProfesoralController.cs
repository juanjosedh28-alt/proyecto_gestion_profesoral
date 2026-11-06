using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>
/// Endpoints HTTP de <c>apoyo_profesoral</c>. Solo traduce HTTP ↔ servicio: no tiene
/// try/catch porque los errores los convierten los middlewares (ManejadorErrores
/// y ManejadorErroresSql).
/// </summary>
[ApiController]
[Route("api/apoyo-profesoral")]
public class ApoyoProfesoralController : ControllerBase
{
    private readonly IServicioApoyoProfesoral _servicio;

    public ApoyoProfesoralController(IServicioApoyoProfesoral servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<ApoyoProfesoral>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{estudios:int}")]
    public async Task<ActionResult<ApoyoProfesoral>> Obtener(int estudios) => Ok(await _servicio.ObtenerAsync(estudios));

    [HttpPost]
    public async Task<ActionResult<ApoyoProfesoral>> Crear([FromBody] ApoyoProfesoralCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/apoyo-profesoral/{creado.Estudios}", creado);
    }

    [HttpPut("{estudios:int}")]
    public async Task<ActionResult<ApoyoProfesoral>> Reemplazar(int estudios, [FromBody] ApoyoProfesoralCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(estudios, peticion));

    [HttpDelete("{estudios:int}")]
    public async Task<IActionResult> Eliminar(int estudios)
    {
        await _servicio.EliminarAsync(estudios);
        return NoContent();
    }
}
