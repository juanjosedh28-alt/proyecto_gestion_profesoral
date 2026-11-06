using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>red_docente</c>.</summary>
public interface IServicioRedDocente
{
    Task<List<RedDocente>> ListarAsync();
    Task<List<RedDocente>> ListarPorDocenteAsync(int cedula);
    Task<RedDocente> ObtenerAsync(int red, int docente);
    Task<RedDocente> CrearAsync(RedDocenteCrear p);
    Task<RedDocente> ActualizarAsync(int red, int docente, RedDocenteCrear p);
    Task EliminarAsync(int red, int docente);
}

/// <summary>
/// Servicio (capa de negocio) de <c>red_docente</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioRedDocente : IServicioRedDocente
{
    private readonly IRepositorioRedDocente _repo;

    public ServicioRedDocente(IRepositorioRedDocente repo) => _repo = repo;

    public Task<List<RedDocente>> ListarAsync() => _repo.ListarAsync();

    public Task<List<RedDocente>> ListarPorDocenteAsync(int cedula) => _repo.ListarPorDocenteAsync(cedula);

    public async Task<RedDocente> ObtenerAsync(int red, int docente) =>
        await _repo.ObtenerAsync(red, docente) ?? throw new NoEncontradoExcepcion($"No se encontró la participación del docente {docente} en la red {red}.");

    public Task<RedDocente> CrearAsync(RedDocenteCrear p)
    {
        Validar(p);
        return _repo.CrearAsync(p);
    }

    public Task<RedDocente> ActualizarAsync(int red, int docente, RedDocenteCrear p)
    {
        if (red != p.Red)
            throw new ValidacionExcepcion($"La red de la ruta ({red}) no coincide con el valor del cuerpo ({p.Red}).");
        if (docente != p.Docente)
            throw new ValidacionExcepcion($"El docente de la ruta ({docente}) no coincide con el valor del cuerpo ({p.Docente}).");
        Validar(p);
        return _repo.ActualizarAsync(p);
    }

    public Task EliminarAsync(int red, int docente) => _repo.EliminarAsync(red, docente);

    /// <summary>Lo que el DTO no puede revisar solo: que las fechas existan y estén en orden.</summary>
    private static void Validar(RedDocenteCrear p, string prefijo = "")
    {
        Validaciones.Revisar(p.FechaInicio, prefijo + "fecha_inicio", "La fecha de inicio");
        Validaciones.Orden(p.FechaInicio, prefijo + "fecha_inicio", p.FechaFin, prefijo + "fecha_fin", "La fecha de fin no puede ser anterior a la fecha de inicio.");
    }
}
