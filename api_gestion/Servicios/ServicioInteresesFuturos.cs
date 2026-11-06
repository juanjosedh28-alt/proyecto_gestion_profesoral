using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>intereses_futuros</c>.</summary>
public interface IServicioInteresesFuturos
{
    Task<List<InteresesFuturos>> ListarAsync();
    Task<List<InteresesFuturos>> ListarPorDocenteAsync(int cedula);
    Task<InteresesFuturos> ObtenerAsync(int docente, string terminoClave);
    Task<InteresesFuturos> CrearAsync(InteresesFuturosCrear p);
    Task EliminarAsync(int docente, string terminoClave);
}

/// <summary>
/// Servicio (capa de negocio) de <c>intereses_futuros</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioInteresesFuturos : IServicioInteresesFuturos
{
    private readonly IRepositorioInteresesFuturos _repo;

    public ServicioInteresesFuturos(IRepositorioInteresesFuturos repo) => _repo = repo;

    public Task<List<InteresesFuturos>> ListarAsync() => _repo.ListarAsync();

    public Task<List<InteresesFuturos>> ListarPorDocenteAsync(int cedula) => _repo.ListarPorDocenteAsync(cedula);

    public async Task<InteresesFuturos> ObtenerAsync(int docente, string terminoClave) =>
        await _repo.ObtenerAsync(docente, terminoClave) ?? throw new NoEncontradoExcepcion($"El docente {docente} no tiene el interés «{terminoClave}».");

    // Si el padre no existe o la pareja ya está, lo dice el procedimiento (409).
    public Task<InteresesFuturos> CrearAsync(InteresesFuturosCrear p) => _repo.CrearAsync(p);

    public Task EliminarAsync(int docente, string terminoClave) => _repo.EliminarAsync(docente, terminoClave);
}
