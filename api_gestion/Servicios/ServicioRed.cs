using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>red</c>.</summary>
public interface IServicioRed
{
    Task<List<Red>> ListarAsync();
    Task<Red> ObtenerAsync(int clave);
    Task<Red> CrearAsync(RedCrear p);
    Task<Red> ActualizarAsync(int idr, RedCrear p);
    Task EliminarAsync(int clave);
}

/// <summary>Servicio (capa de negocio) de <c>red</c>: valida reglas, detecta conflictos y lanza excepciones que la API traduce a 404/409/422.</summary>
public class ServicioRed : IServicioRed
{
    private readonly IRepositorioRed _repo;

    public ServicioRed(IRepositorioRed repo) => _repo = repo;

    public Task<List<Red>> ListarAsync() => _repo.ListarAsync();

    public async Task<Red> ObtenerAsync(int clave) =>
        await _repo.ObtenerAsync(clave) ?? throw new NoEncontradoExcepcion($"No se encontró el red con id {clave}.");

    public async Task<Red> CrearAsync(RedCrear p)
    {
        if (await _repo.ExisteAsync(p.Idr)) throw new ConflictoExcepcion($"Ya existe un red con id {p.Idr}.");
        var entidad = Mapear(p);
        await _repo.CrearAsync(entidad);
        return entidad;
    }

    public async Task<Red> ActualizarAsync(int idr, RedCrear p)
    {
        if (idr != p.Idr) throw new ValidacionExcepcion($"El id de la ruta ({idr}) no coincide con el id del cuerpo ({p.Idr}).");
        var entidad = Mapear(p);
        if (!await _repo.ActualizarAsync(entidad)) throw new NoEncontradoExcepcion($"No se encontró el red con id {idr}.");
        return entidad;
    }

    public async Task EliminarAsync(int clave)
    {
        if (!await _repo.EliminarLogicoAsync(clave)) throw new NoEncontradoExcepcion($"No se encontró el red con id {clave}.");
    }

    private static Red Mapear(RedCrear p) => new()
    {
            Idr = p.Idr,
            Nombre = p.Nombre,
            Url = p.Url,
            Pais = p.Pais,
    };
}
