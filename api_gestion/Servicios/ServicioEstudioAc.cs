using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>estudio_ac</c>.</summary>
public interface IServicioEstudioAc
{
    Task<List<EstudioAc>> ListarAsync();
    Task<List<EstudioAc>> ListarPorEstudioAsync(int id);
    Task<EstudioAc> ObtenerAsync(int estudio, string areaConocimiento);
    Task<EstudioAc> CrearAsync(EstudioAcCrear p);
    Task EliminarAsync(int estudio, string areaConocimiento);
}

/// <summary>
/// Servicio (capa de negocio) de <c>estudio_ac</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioEstudioAc : IServicioEstudioAc
{
    private readonly IRepositorioEstudioAc _repo;

    public ServicioEstudioAc(IRepositorioEstudioAc repo) => _repo = repo;

    public Task<List<EstudioAc>> ListarAsync() => _repo.ListarAsync();

    public Task<List<EstudioAc>> ListarPorEstudioAsync(int id) => _repo.ListarPorEstudioAsync(id);

    public async Task<EstudioAc> ObtenerAsync(int estudio, string areaConocimiento) =>
        await _repo.ObtenerAsync(estudio, areaConocimiento) ?? throw new NoEncontradoExcepcion($"El estudio {estudio} no tiene el área {areaConocimiento}.");

    // Si el padre no existe o la pareja ya está, lo dice el procedimiento (409).
    public Task<EstudioAc> CrearAsync(EstudioAcCrear p) => _repo.CrearAsync(p);

    public Task EliminarAsync(int estudio, string areaConocimiento) => _repo.EliminarAsync(estudio, areaConocimiento);
}
