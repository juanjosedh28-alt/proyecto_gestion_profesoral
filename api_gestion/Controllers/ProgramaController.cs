<<<<<<< HEAD
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

[ApiController]
[Route("api/[controller]")]
=======
using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>Endpoints HTTP de <c>programa</c>. Solo traduce HTTP ↔ servicio; los errores los convierte el middleware.</summary>
[ApiController]
[Route("api/programa")]
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
public class ProgramaController : ControllerBase
{
    private readonly IServicioPrograma _servicio;

<<<<<<< HEAD
    public ProgramaController(IServicioPrograma servicio)
    {
        _servicio = servicio;
    }

    [HttpGet]
    public async Task<IActionResult> Listar()
    {
        var lista = await _servicio.ListarAsync();
        return Ok(lista);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        var programa = await _servicio.ObtenerAsync(id);
        if (programa == null) return NotFound(new { mensaje = "Programa no encontrado." });
        return Ok(programa);
    }

    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProgramaCrear peticion)
    {
        var programa = new Programa
        {
            Nombre = peticion.Nombre,
            Codigo = peticion.Codigo
        };
        var id = await _servicio.CrearAsync(programa);
        return CreatedAtAction(nameof(Obtener), new { id }, new { id, programa.Nombre, programa.Codigo });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Actualizar(int id, [FromBody] ProgramaCrear peticion)
    {
        var programa = new Programa
        {
            Id = id,
            Nombre = peticion.Nombre,
            Codigo = peticion.Codigo
        };
        var actualizado = await _servicio.ActualizarAsync(programa);
        if (!actualizado) return NotFound(new { mensaje = "Programa no encontrado o inactivo." });
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        var eliminado = await _servicio.EliminarAsync(id);
        if (!eliminado) return NotFound(new { mensaje = "Programa no encontrado." });
=======
    public ProgramaController(IServicioPrograma servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<Programa>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id:int}")]
    public async Task<ActionResult<Programa>> Obtener(int id) => Ok(await _servicio.ObtenerAsync(id));

    [HttpPost]
    public async Task<ActionResult<Programa>> Crear([FromBody] ProgramaCrear peticion)
    {
        var creado = await _servicio.CrearAsync(peticion);
        return Created($"/api/programa/{creado.Id}", creado);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<Programa>> Reemplazar(int id, [FromBody] ProgramaCrear peticion) =>
        Ok(await _servicio.ActualizarAsync(id, peticion));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        await _servicio.EliminarAsync(id);
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
        return NoContent();
    }
}
