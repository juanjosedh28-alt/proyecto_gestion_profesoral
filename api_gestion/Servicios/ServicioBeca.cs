using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>beca</c>.</summary>
public interface IServicioBeca
{
    Task<List<Beca>> ListarAsync();
    Task<Beca> ObtenerAsync(int estudios);
    Task<Beca> CrearAsync(BecaCrear p);
    Task<Beca> ActualizarAsync(int estudios, BecaCrear p);
    Task EliminarAsync(int estudios);
}

/// <summary>
/// Servicio (capa de negocio) de <c>beca</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioBeca : IServicioBeca
{
    private readonly IRepositorioBeca _repo;

    public ServicioBeca(IRepositorioBeca repo) => _repo = repo;

    public Task<List<Beca>> ListarAsync() => _repo.ListarAsync();

    public async Task<Beca> ObtenerAsync(int estudios) =>
        await _repo.ObtenerAsync(estudios) ?? throw new NoEncontradoExcepcion($"No se encontró la beca del estudio {estudios}.");

    public Task<Beca> CrearAsync(BecaCrear p)
    {
        Validar(p);
        return _repo.CrearAsync(p);
    }

    public Task<Beca> ActualizarAsync(int estudios, BecaCrear p)
    {
        if (estudios != p.Estudios)
            throw new ValidacionExcepcion($"El estudio de la ruta ({estudios}) no coincide con el valor del cuerpo ({p.Estudios}).");
        Validar(p);
        return _repo.ActualizarAsync(p);
    }

    public Task EliminarAsync(int estudios) => _repo.EliminarAsync(estudios);

    /// <summary>Lo que el DTO no puede revisar solo: que las fechas existan y estén en orden.</summary>
    private static void Validar(BecaCrear p, string prefijo = "")
    {
        Validaciones.Revisar(p.FechaInicio, prefijo + "fecha_inicio", "La fecha de inicio");
        Validaciones.Orden(p.FechaInicio, prefijo + "fecha_inicio", p.FechaFin, prefijo + "fecha_fin", "La fecha de fin no puede ser anterior a la fecha de inicio.");
    }
}
