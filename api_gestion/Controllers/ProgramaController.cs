using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Servicios;
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

    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] int limite = 100)
    {
        var datos = await _servicio.ObtenerTodosAsync(limite);
        if (datos.Count == 0) return NoContent(); // HTTP 204 si la tabla está vacía
        return Ok(new { recurso = "programa", limite, total = datos.Count, datos });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try { return Ok(await _servicio.ObtenerPorIdAsync(id)); }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { mensaje = ex.Message }); }
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ProgramaCrear peticion)
    {
        if (!ModelState.IsValid) return UnprocessableEntity(ModelState); // HTTP 422
        var entidad = MapearCrearAEntidad(peticion.Id!.Value, peticion);
        await _servicio.CrearAsync(entidad);
        return Ok(entidad);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, [FromBody] ProgramaCrear peticion)
    {
        if (!ModelState.IsValid) return UnprocessableEntity(ModelState); // HTTP 422
        try
        {
            var entidad = MapearCrearAEntidad(id, peticion);
            await _servicio.ActualizarAsync(entidad);
            return Ok(entidad);
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { mensaje = ex.Message }); }
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> Patch(int id, [FromBody] ProgramaActualizar peticion)
    {
        try
        {
            var actual = await _servicio.ObtenerPorIdAsync(id);
            if (peticion.Nombre != null) actual.Nombre = peticion.Nombre;
            if (peticion.Tipo != null) actual.Tipo = peticion.Tipo;
            if (peticion.Nivel != null) actual.Nivel = peticion.Nivel;
            if (peticion.FechaCreacion != null) actual.FechaCreacion = peticion.FechaCreacion;
            if (peticion.FechaCierre != null) actual.FechaCierre = peticion.FechaCierre;
            if (peticion.NumeroCohortes != null) actual.NumeroCohortes = peticion.NumeroCohortes;
            if (peticion.CantGraduados != null) actual.CantGraduados = peticion.CantGraduados;
            if (peticion.FechaActualizacion != null) actual.FechaActualizacion = peticion.FechaActualizacion;
            if (peticion.Ciudad != null) actual.Ciudad = peticion.Ciudad;
            if (peticion.Facultad.HasValue) actual.Facultad = peticion.Facultad.Value;

            await _servicio.ActualizarAsync(actual);
            return Ok(actual); // HTTP 200
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { mensaje = ex.Message }); }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _servicio.EliminarAsync(id);
            return Ok(new { mensaje = "Programa eliminado correctamente (borrado lógico)." });
        }
        catch (NoEncontradoExcepcion ex) { return NotFound(new { mensaje = ex.Message }); }
    }

    private static Programa MapearCrearAEntidad(int id, ProgramaCrear p) => new()
    {
        Id = id,
        Nombre = p.Nombre!,
        Tipo = p.Tipo!,
        Nivel = p.Nivel!,
        FechaCreacion = p.FechaCreacion!,
        FechaCierre = p.FechaCierre,
        NumeroCohortes = p.NumeroCohortes!,
        CantGraduados = p.CantGraduados!,
        FechaActualizacion = p.FechaActualizacion!,
        Ciudad = p.Ciudad!,
        Facultad = p.Facultad!.Value
    };
}