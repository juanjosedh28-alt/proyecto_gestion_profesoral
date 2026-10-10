using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>termino_clave</c>.</summary>
public interface IServicioTerminoClave
{
    Task<List<TerminoClave>> ListarAsync();
    Task<TerminoClave> ObtenerAsync(string clave);
    Task<TerminoClave> CrearAsync(TerminoClaveCrear p);
    Task<TerminoClave> ActualizarAsync(string clave, TerminoClaveCrear p);
    Task EliminarAsync(string clave);
}

/// <summary>Servicio (capa de negocio) de <c>termino_clave</c>: valida reglas, detecta conflictos y lanza excepciones que la API traduce a 404/409/422.</summary>
public class ServicioTerminoClave : IServicioTerminoClave
{
    private readonly IRepositorioTerminoClave _repo;

    public ServicioTerminoClave(IRepositorioTerminoClave repo) => _repo = repo;

    public Task<List<TerminoClave>> ListarAsync() => _repo.ListarAsync();

    public async Task<TerminoClave> ObtenerAsync(string clave) =>
        await _repo.ObtenerAsync(clave) ?? throw new NoEncontradoExcepcion($"No se encontró el término clave con término {clave}.");

    public async Task<TerminoClave> CrearAsync(TerminoClaveCrear p)
    {
        if (string.IsNullOrWhiteSpace(p.Termino)) throw new ValidacionExcepcion("Datos inválidos.", "termino: El término es obligatorio.");
        if (await _repo.ExisteAsync(p.Termino)) throw new ConflictoExcepcion($"Ya existe un término clave con término {p.Termino}.");
        var entidad = Mapear(p);
        await _repo.CrearAsync(entidad);
        return entidad;
    }

    public async Task<TerminoClave> ActualizarAsync(string clave, TerminoClaveCrear p)
    {
        if (!string.IsNullOrEmpty(p.Termino) && p.Termino != clave) throw new ValidacionExcepcion($"El término de la ruta ({clave}) no coincide con el término del cuerpo ({p.Termino}).");
        var entidad = Mapear(p);
        entidad.Termino = clave;
        if (!await _repo.ActualizarAsync(entidad)) throw new NoEncontradoExcepcion($"No se encontró el término clave con término {clave}.");
        return entidad;
    }

    public async Task EliminarAsync(string clave)
    {
        if (!await _repo.EliminarLogicoAsync(clave)) throw new NoEncontradoExcepcion($"No se encontró el término clave con término {clave}.");
    }

    private static TerminoClave Mapear(TerminoClaveCrear p) => new()
    {
            Termino = p.Termino ?? string.Empty,
            TerminoIngles = p.TerminoIngles,
    };
}
