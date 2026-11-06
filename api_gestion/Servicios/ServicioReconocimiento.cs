using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>reconocimiento</c>.</summary>
public interface IServicioReconocimiento
{
    Task<List<Reconocimiento>> ListarAsync();
    Task<List<Reconocimiento>> ListarPorDocenteAsync(int cedula);
    Task<Reconocimiento> ObtenerAsync(int id);
    Task<Reconocimiento> CrearAsync(ReconocimientoCrear p);
    Task<List<Reconocimiento>> CrearLoteAsync(int cedula, List<ReconocimientoRenglon> renglones);
    Task<Reconocimiento> ActualizarAsync(int id, ReconocimientoCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Servicio (capa de negocio) de <c>reconocimiento</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioReconocimiento : IServicioReconocimiento
{
    private readonly IRepositorioReconocimiento _repo;

    public ServicioReconocimiento(IRepositorioReconocimiento repo) => _repo = repo;

    public Task<List<Reconocimiento>> ListarAsync() => _repo.ListarAsync();

    public Task<List<Reconocimiento>> ListarPorDocenteAsync(int cedula) => _repo.ListarPorDocenteAsync(cedula);

    public async Task<Reconocimiento> ObtenerAsync(int id) =>
        await _repo.ObtenerAsync(id) ?? throw new NoEncontradoExcepcion($"No se encontró el reconocimiento con id {id}.");

    public Task<Reconocimiento> CrearAsync(ReconocimientoCrear p)
    {
        Validar(p);
        return _repo.CrearAsync(p);
    }

    // Envío múltiple: de 1 a 100 renglones; si uno falla no se guarda ninguno.
    public Task<List<Reconocimiento>> CrearLoteAsync(int cedula, List<ReconocimientoRenglon> renglones)
    {
        Validaciones.TamanoLote(renglones.Count);
        for (var i = 0; i < renglones.Count; i++)
            Validar(renglones[i], $"renglón {i + 1}, ");
        return _repo.CrearLoteAsync(cedula, renglones);
    }

    public Task<Reconocimiento> ActualizarAsync(int id, ReconocimientoCrear p)
    {
        Validar(p);
        return _repo.ActualizarAsync(id, p);
    }

    public Task EliminarAsync(int id) => _repo.EliminarAsync(id);

    /// <summary>Lo que el DTO no puede revisar solo: que las fechas existan y estén en orden.</summary>
    private static void Validar(ReconocimientoRenglon p, string prefijo = "")
    {
        Validaciones.Revisar(p.Fecha, prefijo + "fecha", "La fecha");
    }
}
