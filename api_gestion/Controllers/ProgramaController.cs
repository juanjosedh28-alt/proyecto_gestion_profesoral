using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Servicios;
using ApiGestion.Peticiones;
using ApiGestion.Excepciones;

namespace ApiGestion.Controllers;

[ApiController]
[Route("api/programa")]
public class ProgramaController : ControllerBase
{
    private readonly IServicioPrograma _servicio;

    public ProgramaController(IServicioPrograma servicio)
    {
        _servicio = servicio;
    }

    // RF1 — Listar todos los programas activos
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int limite = 1000)
    {
        var datos = await _servicio.ObtenerTodosAsync(limite);
        if (datos.Count == 0) return NoContent(); // Código 204 si está vacío
        return Ok(new { tabla = "programa", limite, total = datos.Count, datos });
    }

    // RF2 — Obtener un programa por su código ID
    [HttpGet("{id}")]
    public async Task<IActionResult> Obtener(int id)
    {
        try
        {
            var programa = await _servicio.ObtenerPorIdAsync(id);
            return Ok(programa);
        }
        catch (NoEncontradoExcepcion e)
        {
            return NotFound(new { estado = 404, mensaje = e.Message });
        }
    }

    // RF3 — Crear un nuevo programa
    [HttpPost]
    public async Task<IActionResult> Crear([FromBody] ProgramaCrear peticion)
    {
        if (!ModelState.IsValid) return UnprocessableEntity(ModelState);

        var entidad = new Programa
        {
            Id = peticion.Id ?? 0,
            Nombre = peticion.Nombre!,
            Tipo = peticion.Tipo!,
            Nivel = peticion.Nivel!,
            FechaCreacion = peticion.FechaCreacion!,
            FechaCierre = peticion.FechaCierre,
            NumeroCohortes = peticion.NumeroCohortes!,
            CantGraduados = peticion.CantGraduados!,
            FechaActualizacion = peticion.FechaActualizacion!,
            Ciudad = peticion.Ciudad!,
            Facultad = peticion.Facultad ?? 0
        };

        await _servicio.CrearAsync(entidad);
        return Ok(new { mensaje = "Creado exitosamente" });
    }

    // RF6 — Retirar (borrado lógico: activo = 0)
    [HttpDelete("{id}")]
    public async Task<IActionResult> Eliminar(int id)
    {
        try
        {
            await _servicio.EliminarAsync(id);
            return Ok(new { mensaje = "Retirado exitosamente" });
        }
        catch (NoEncontradoExcepcion e)
        {
            return NotFound(new { estado = 404, mensaje = e.Message });
        }
    }
}