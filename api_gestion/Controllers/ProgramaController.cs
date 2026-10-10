using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;
using Microsoft.AspNetCore.Mvc;

namespace ApiGestion.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProgramaController : ControllerBase
{
    private readonly IServicioPrograma _servicio;

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
        return NoContent();
    }
}
