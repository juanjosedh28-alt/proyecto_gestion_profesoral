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
        // Si el término ya existe, el procedimiento sp_crear_termino_clave responde 409.
        return await _repo.CrearAsync(Mapear(p));
    }

    public async Task<TerminoClave> ActualizarAsync(string clave, TerminoClaveCrear p)
    {
        if (!string.IsNullOrEmpty(p.Termino) && p.Termino != clave) throw new ValidacionExcepcion($"El término de la ruta ({clave}) no coincide con el término del cuerpo ({p.Termino}).");
        var entidad = Mapear(p);
        entidad.Termino = clave;
        return await _repo.ActualizarAsync(entidad);   // 404 lo responde el procedimiento
    }

    public Task EliminarAsync(string clave) => _repo.EliminarLogicoAsync(clave);

    private static TerminoClave Mapear(TerminoClaveCrear p) => new()
    {
            Termino = p.Termino ?? string.Empty,
            TerminoIngles = p.TerminoIngles,
    };
}
