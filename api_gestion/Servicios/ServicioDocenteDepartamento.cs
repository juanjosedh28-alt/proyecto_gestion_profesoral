using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>docente_departamento</c>.</summary>
public interface IServicioDocenteDepartamento
{
    Task<List<DocenteDepartamento>> ListarAsync();
    Task<List<DocenteDepartamento>> ListarPorDocenteAsync(int cedula);
    Task<DocenteDepartamento> ObtenerAsync(int docente, int departamento);
    Task<DocenteDepartamento> CrearAsync(DocenteDepartamentoCrear p);
    Task<DocenteDepartamento> ActualizarAsync(int docente, int departamento, DocenteDepartamentoCrear p);
    Task EliminarAsync(int docente, int departamento);
}

/// <summary>
/// Servicio (capa de negocio) de <c>docente_departamento</c>.
/// Revisa lo que se puede saber SIN ir a la base (fechas reales, claves de la ruta
/// iguales a las del cuerpo, tamaño del envío) y lanza excepciones que la API
/// traduce a 404/422. Lo que depende de otras tablas (que el padre exista, que
/// no esté repetido, que se pueda retirar) lo revisa el procedimiento almacenado:
/// así hay una sola fuente de verdad.
/// </summary>
public class ServicioDocenteDepartamento : IServicioDocenteDepartamento
{
    private readonly IRepositorioDocenteDepartamento _repo;

    public ServicioDocenteDepartamento(IRepositorioDocenteDepartamento repo) => _repo = repo;

    public Task<List<DocenteDepartamento>> ListarAsync() => _repo.ListarAsync();

    public Task<List<DocenteDepartamento>> ListarPorDocenteAsync(int cedula) => _repo.ListarPorDocenteAsync(cedula);

    public async Task<DocenteDepartamento> ObtenerAsync(int docente, int departamento) =>
        await _repo.ObtenerAsync(docente, departamento) ?? throw new NoEncontradoExcepcion($"No se encontró la asignación del docente {docente} al programa {departamento}.");

    public Task<DocenteDepartamento> CrearAsync(DocenteDepartamentoCrear p)
    {
        Validar(p);
        return _repo.CrearAsync(p);
    }

    public Task<DocenteDepartamento> ActualizarAsync(int docente, int departamento, DocenteDepartamentoCrear p)
    {
        if (docente != p.Docente)
            throw new ValidacionExcepcion($"El docente de la ruta ({docente}) no coincide con el valor del cuerpo ({p.Docente}).");
        if (departamento != p.Departamento)
            throw new ValidacionExcepcion($"El programa de la ruta ({departamento}) no coincide con el valor del cuerpo ({p.Departamento}).");
        Validar(p);
        return _repo.ActualizarAsync(p);
    }

    public Task EliminarAsync(int docente, int departamento) => _repo.EliminarAsync(docente, departamento);

    /// <summary>Lo que el DTO no puede revisar solo: que las fechas existan y estén en orden.</summary>
    private static void Validar(DocenteDepartamentoCrear p, string prefijo = "")
    {
        Validaciones.Revisar(p.FechaIngreso, prefijo + "fecha_ingreso", "La fecha de ingreso");
        Validaciones.Orden(p.FechaIngreso, prefijo + "fecha_ingreso", p.FechaSalida, prefijo + "fecha_salida", "La fecha de salida no puede ser anterior a la fecha de ingreso.");
    }
}
