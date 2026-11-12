using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>evaluacion_docente</c>.</summary>
public interface IServicioEvaluacionDocente
{
    Task<List<EvaluacionDocente>> ListarAsync();
    Task<List<EvaluacionDocente>> ListarPorDocenteAsync(int cedula);
    Task<EvaluacionDocente> ObtenerAsync(int id);
    Task<EvaluacionDocente> CrearAsync(EvaluacionDocenteCrear p);
    Task<List<EvaluacionDocente>> CrearLoteAsync(int cedula, List<EvaluacionDocenteRenglon> renglones);
    Task<EvaluacionDocente> ActualizarAsync(int id, EvaluacionDocenteCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Servicio (capa de negocio) de <c>evaluacion_docente</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioEvaluacionDocente : IServicioEvaluacionDocente
{
    private readonly IRepositorioEvaluacionDocente _repo;

    public ServicioEvaluacionDocente(IRepositorioEvaluacionDocente repo) => _repo = repo;

    public Task<List<EvaluacionDocente>> ListarAsync() => _repo.ListarAsync();

    public Task<List<EvaluacionDocente>> ListarPorDocenteAsync(int cedula) => _repo.ListarPorDocenteAsync(cedula);

    public async Task<EvaluacionDocente> ObtenerAsync(int id) =>
        await _repo.ObtenerAsync(id) ?? throw new NoEncontradoExcepcion($"No se encontró la evaluación con id {id}.");

    // Si el padre no existe o la pareja ya está, lo dice el procedimiento (409).
    public Task<EvaluacionDocente> CrearAsync(EvaluacionDocenteCrear p) => _repo.CrearAsync(p);

    // Envío múltiple: de 1 a 100 renglones; si uno falla no se guarda ninguno.
    public Task<List<EvaluacionDocente>> CrearLoteAsync(int cedula, List<EvaluacionDocenteRenglon> renglones)
    {
        Validaciones.TamanoLote(renglones.Count);
        // Cada renglón ya lo validaron las anotaciones de la petición.
        return _repo.CrearLoteAsync(cedula, renglones);
    }

    public Task<EvaluacionDocente> ActualizarAsync(int id, EvaluacionDocenteCrear p)
    {
        // Las reglas que miran otras tablas las revisa el procedimiento.
        return _repo.ActualizarAsync(id, p);
    }

    public Task EliminarAsync(int id) => _repo.EliminarAsync(id);
}
