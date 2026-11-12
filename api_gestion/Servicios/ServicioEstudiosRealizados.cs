using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>estudios_realizados</c>.</summary>
public interface IServicioEstudiosRealizados
{
    Task<List<EstudiosRealizados>> ListarAsync();
    Task<List<EstudiosRealizados>> ListarPorDocenteAsync(int cedula);
    Task<EstudiosRealizados> ObtenerAsync(int id);
    Task<EstudiosRealizados> CrearAsync(EstudiosRealizadosCrear p);
    Task<List<EstudiosRealizados>> CrearLoteAsync(int cedula, List<EstudiosRealizadosRenglon> renglones);
    Task<EstudiosRealizados> ActualizarAsync(int id, EstudiosRealizadosCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Servicio (capa de negocio) de <c>estudios_realizados</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioEstudiosRealizados : IServicioEstudiosRealizados
{
    private readonly IRepositorioEstudiosRealizados _repo;

    public ServicioEstudiosRealizados(IRepositorioEstudiosRealizados repo) => _repo = repo;

    public Task<List<EstudiosRealizados>> ListarAsync() => _repo.ListarAsync();

    public Task<List<EstudiosRealizados>> ListarPorDocenteAsync(int cedula) => _repo.ListarPorDocenteAsync(cedula);

    public async Task<EstudiosRealizados> ObtenerAsync(int id) =>
        await _repo.ObtenerAsync(id) ?? throw new NoEncontradoExcepcion($"No se encontró el estudio con id {id}.");

    public Task<EstudiosRealizados> CrearAsync(EstudiosRealizadosCrear p)
    {
        Validar(p);
        return _repo.CrearAsync(p);
    }

    // Envío múltiple: de 1 a 100 renglones; si uno falla no se guarda ninguno.
    public Task<List<EstudiosRealizados>> CrearLoteAsync(int cedula, List<EstudiosRealizadosRenglon> renglones)
    {
        Validaciones.TamanoLote(renglones.Count);
        for (var i = 0; i < renglones.Count; i++)
            Validar(renglones[i], $"renglón {i + 1}, ");
        return _repo.CrearLoteAsync(cedula, renglones);
    }

    public Task<EstudiosRealizados> ActualizarAsync(int id, EstudiosRealizadosCrear p)
    {
        if (id != p.Id)
            throw new ValidacionExcepcion($"El id de la ruta ({id}) no coincide con el valor del cuerpo ({p.Id}).");
        Validar(p);
        return _repo.ActualizarAsync(p);
    }

    public Task EliminarAsync(int id) => _repo.EliminarAsync(id);

    /// <summary>Lo que el DTO no puede revisar solo: que las fechas existan y estén en orden.</summary>
    private static void Validar(EstudiosRealizadosRenglon p, string prefijo = "")
    {
        Validaciones.Revisar(p.Fecha, prefijo + "fecha", "La fecha");
    }
}
