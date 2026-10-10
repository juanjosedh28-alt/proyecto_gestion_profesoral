using Microsoft.AspNetCore.Mvc;
using ApiGestion.Modelos;
using ApiGestion.Servicios;

namespace ApiGestion.Controllers;

/// <summary>Endpoints HTTP de solo lectura de <c>area_conocimiento</c> (catálogo precargado).</summary>
[ApiController]
[Route("api/area-conocimiento")]
public class AreaConocimientoController : ControllerBase
{
    private readonly IServicioAreaConocimiento _servicio;

    public AreaConocimientoController(IServicioAreaConocimiento servicio) => _servicio = servicio;

    [HttpGet]
    public async Task<ActionResult<List<AreaConocimiento>>> Listar() => Ok(await _servicio.ListarAsync());

    [HttpGet("{id}")]
    public async Task<ActionResult<AreaConocimiento>> Obtener(string id) => Ok(await _servicio.ObtenerAsync(id));
}
