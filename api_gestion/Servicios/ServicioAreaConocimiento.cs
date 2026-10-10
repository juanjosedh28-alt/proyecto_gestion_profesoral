using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las consultas de <c>area_conocimiento</c> (solo lectura).</summary>
public interface IServicioAreaConocimiento
{
    Task<List<AreaConocimiento>> ListarAsync();
    Task<AreaConocimiento> ObtenerAsync(string clave);
}

/// <summary>Servicio (capa de negocio) de <c>area_conocimiento</c>.</summary>
public class ServicioAreaConocimiento : IServicioAreaConocimiento
{
    private readonly IRepositorioAreaConocimiento _repo;

    public ServicioAreaConocimiento(IRepositorioAreaConocimiento repo) => _repo = repo;

    public Task<List<AreaConocimiento>> ListarAsync() => _repo.ListarAsync();

    public async Task<AreaConocimiento> ObtenerAsync(string clave) =>
        await _repo.ObtenerAsync(clave) ?? throw new NoEncontradoExcepcion($"No se encontró el área de conocimiento con id {clave}.");
}
