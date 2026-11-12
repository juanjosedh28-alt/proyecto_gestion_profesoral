using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>docente</c>.</summary>
public interface IServicioDocente
{
    Task<List<Docente>> ListarAsync();
    Task<Docente> ObtenerAsync(int cedula);
    Task<Docente> CrearAsync(DocenteCrear p);
    Task<Docente> ActualizarAsync(int cedula, DocenteCrear p);
    Task EliminarAsync(int cedula);
}

/// <summary>
/// Servicio (capa de negocio) de <c>docente</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioDocente : IServicioDocente
{
    private readonly IRepositorioDocente _repo;

    public ServicioDocente(IRepositorioDocente repo) => _repo = repo;

    public Task<List<Docente>> ListarAsync() => _repo.ListarAsync();

    public async Task<Docente> ObtenerAsync(int cedula) =>
        await _repo.ObtenerAsync(cedula) ?? throw new NoEncontradoExcepcion($"No se encontró el docente con cédula {cedula}.");

    public Task<Docente> CrearAsync(DocenteCrear p)
    {
        Validar(p);
        return _repo.CrearAsync(p);
    }

    public Task<Docente> ActualizarAsync(int cedula, DocenteCrear p)
    {
        if (cedula != p.Cedula)
            throw new ValidacionExcepcion($"La cédula de la ruta ({cedula}) no coincide con el valor del cuerpo ({p.Cedula}).");
        Validar(p);
        return _repo.ActualizarAsync(p);
    }

    public Task EliminarAsync(int cedula) => _repo.EliminarAsync(cedula);

    /// <summary>Lo que el DTO no puede revisar solo: que las fechas existan y estén en orden.</summary>
    private static void Validar(DocenteCrear p, string prefijo = "")
    {
        Validaciones.NoFutura(p.FechaNacimiento, prefijo + "fecha_nacimiento", "La fecha de nacimiento");
    }
}
