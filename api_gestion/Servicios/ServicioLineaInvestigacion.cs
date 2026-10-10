using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>linea_investigacion</c>.</summary>
public interface IServicioLineaInvestigacion
{
    Task<List<LineaInvestigacion>> ListarAsync();
    Task<LineaInvestigacion> ObtenerAsync(int clave);
    Task<LineaInvestigacion> CrearAsync(LineaInvestigacionCrear p);
    Task<LineaInvestigacion> ActualizarAsync(int id, LineaInvestigacionCrear p);
    Task EliminarAsync(int clave);
}

/// <summary>Servicio (capa de negocio) de <c>linea_investigacion</c>: valida reglas, detecta conflictos y lanza excepciones que la API traduce a 404/409/422.</summary>
public class ServicioLineaInvestigacion : IServicioLineaInvestigacion
{
    private readonly IRepositorioLineaInvestigacion _repo;

    public ServicioLineaInvestigacion(IRepositorioLineaInvestigacion repo) => _repo = repo;

    public Task<List<LineaInvestigacion>> ListarAsync() => _repo.ListarAsync();

    public async Task<LineaInvestigacion> ObtenerAsync(int clave) =>
        await _repo.ObtenerAsync(clave) ?? throw new NoEncontradoExcepcion($"No se encontró el línea de investigación con id {clave}.");

    public async Task<LineaInvestigacion> CrearAsync(LineaInvestigacionCrear p)
    {
        var entidad = Mapear(p);
        entidad.Id = await _repo.CrearAsync(entidad);
        return entidad;
    }

    public async Task<LineaInvestigacion> ActualizarAsync(int id, LineaInvestigacionCrear p)
    {
        var entidad = Mapear(p);
        entidad.Id = id;
        if (!await _repo.ActualizarAsync(entidad)) throw new NoEncontradoExcepcion($"No se encontró el línea de investigación con id {id}.");
        return entidad;
    }

    public async Task EliminarAsync(int clave)
    {
        if (!await _repo.EliminarLogicoAsync(clave)) throw new NoEncontradoExcepcion($"No se encontró el línea de investigación con id {clave}.");
    }

    private static LineaInvestigacion Mapear(LineaInvestigacionCrear p) => new()
    {
            Nombre = p.Nombre,
            Descripcion = p.Descripcion,
    };
}
