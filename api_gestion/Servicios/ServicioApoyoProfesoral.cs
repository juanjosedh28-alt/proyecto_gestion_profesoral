using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>apoyo_profesoral</c>.</summary>
public interface IServicioApoyoProfesoral
{
    Task<List<ApoyoProfesoral>> ListarAsync();
    Task<ApoyoProfesoral> ObtenerAsync(int estudios);
    Task<ApoyoProfesoral> CrearAsync(ApoyoProfesoralCrear p);
    Task<ApoyoProfesoral> ActualizarAsync(int estudios, ApoyoProfesoralCrear p);
    Task EliminarAsync(int estudios);
}

/// <summary>
/// Servicio (capa de negocio) de <c>apoyo_profesoral</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioApoyoProfesoral : IServicioApoyoProfesoral
{
    private readonly IRepositorioApoyoProfesoral _repo;

    public ServicioApoyoProfesoral(IRepositorioApoyoProfesoral repo) => _repo = repo;

    public Task<List<ApoyoProfesoral>> ListarAsync() => _repo.ListarAsync();

    public async Task<ApoyoProfesoral> ObtenerAsync(int estudios) =>
        await _repo.ObtenerAsync(estudios) ?? throw new NoEncontradoExcepcion($"No se encontró el apoyo profesoral del estudio {estudios}.");

    // Si el padre no existe o la pareja ya está, lo dice el procedimiento (409).
    public Task<ApoyoProfesoral> CrearAsync(ApoyoProfesoralCrear p) => _repo.CrearAsync(p);

    public Task<ApoyoProfesoral> ActualizarAsync(int estudios, ApoyoProfesoralCrear p)
    {
        if (estudios != p.Estudios)
            throw new ValidacionExcepcion($"El estudio de la ruta ({estudios}) no coincide con el valor del cuerpo ({p.Estudios}).");
        return _repo.ActualizarAsync(p);
    }

    public Task EliminarAsync(int estudios) => _repo.EliminarAsync(estudios);
}
