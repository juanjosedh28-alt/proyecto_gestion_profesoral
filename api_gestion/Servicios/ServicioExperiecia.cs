using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>experiecia</c>.</summary>
public interface IServicioExperiecia
{
    Task<List<Experiecia>> ListarAsync();
    Task<List<Experiecia>> ListarPorDocenteAsync(int cedula);
    Task<Experiecia> ObtenerAsync(int id);
    Task<Experiecia> CrearAsync(ExperieciaCrear p);
    Task<List<Experiecia>> CrearLoteAsync(int cedula, List<ExperieciaRenglon> renglones);
    Task<Experiecia> ActualizarAsync(int id, ExperieciaCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Servicio (capa de negocio) de <c>experiecia</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioExperiecia : IServicioExperiecia
{
    private readonly IRepositorioExperiecia _repo;

    public ServicioExperiecia(IRepositorioExperiecia repo) => _repo = repo;

    public Task<List<Experiecia>> ListarAsync() => _repo.ListarAsync();

    public Task<List<Experiecia>> ListarPorDocenteAsync(int cedula) => _repo.ListarPorDocenteAsync(cedula);

    public async Task<Experiecia> ObtenerAsync(int id) =>
        await _repo.ObtenerAsync(id) ?? throw new NoEncontradoExcepcion($"No se encontró la experiencia con id {id}.");

    public Task<Experiecia> CrearAsync(ExperieciaCrear p)
    {
        Validar(p);
        return _repo.CrearAsync(p);
    }

    // Envío múltiple: de 1 a 100 renglones; si uno falla no se guarda ninguno.
    public Task<List<Experiecia>> CrearLoteAsync(int cedula, List<ExperieciaRenglon> renglones)
    {
        Validaciones.TamanoLote(renglones.Count);
        for (var i = 0; i < renglones.Count; i++)
            Validar(renglones[i], $"renglón {i + 1}, ");
        return _repo.CrearLoteAsync(cedula, renglones);
    }

    public Task<Experiecia> ActualizarAsync(int id, ExperieciaCrear p)
    {
        Validar(p);
        return _repo.ActualizarAsync(id, p);
    }

    public Task EliminarAsync(int id) => _repo.EliminarAsync(id);

    /// <summary>Lo que el DTO no puede revisar solo: que las fechas existan y estén en orden.</summary>
    private static void Validar(ExperieciaRenglon p, string prefijo = "")
    {
        Validaciones.Revisar(p.FechaInicio, prefijo + "fecha_inicio", "La fecha de inicio");
        Validaciones.Orden(p.FechaInicio, prefijo + "fecha_inicio", p.FechaFin, prefijo + "fecha_fin", "La fecha de fin no puede ser anterior a la fecha de inicio.");
    }
}
