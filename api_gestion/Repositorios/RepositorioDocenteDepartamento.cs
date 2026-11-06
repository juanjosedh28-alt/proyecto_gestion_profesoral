using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>docente_departamento</c>.</summary>
public interface IRepositorioDocenteDepartamento
{
    Task<List<DocenteDepartamento>> ListarAsync();
    Task<List<DocenteDepartamento>> ListarPorDocenteAsync(int cedula);
    Task<DocenteDepartamento?> ObtenerAsync(int docente, int departamento);
    Task<DocenteDepartamento> CrearAsync(DocenteDepartamentoCrear p);
    Task<DocenteDepartamento> ActualizarAsync(DocenteDepartamentoCrear p);
    Task EliminarAsync(int docente, int departamento);
}

/// <summary>
/// Repositorio (capa de datos) de <c>docente_departamento (PUENTE docente <-> programa, con atributos)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioDocenteDepartamento : IRepositorioDocenteDepartamento
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioDocenteDepartamento(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<DocenteDepartamento>> ListarAsync() =>
        _bd.ListarAsync<DocenteDepartamento>("sp_listar_docente_departamento");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<DocenteDepartamento>> ListarPorDocenteAsync(int cedula) =>
        _bd.ListarAsync<DocenteDepartamento>("sp_listar_docente_departamento", new { docente = cedula });

    public Task<DocenteDepartamento?> ObtenerAsync(int docente, int departamento) =>
        _bd.PrimeroAsync<DocenteDepartamento>("sp_consultar_docente_departamento", new { docente, departamento });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<DocenteDepartamento> CrearAsync(DocenteDepartamentoCrear p) =>
        (await _bd.PrimeroAsync<DocenteDepartamento>("sp_crear_docente_departamento", new
        {
            docente = p.Docente,
            departamento = p.Departamento,
            dedicacion = p.Dedicacion,
            modalidad = p.Modalidad,
            fecha_ingreso = p.FechaIngreso,
            fecha_salida = p.FechaSalida
        }))!;

    public async Task<DocenteDepartamento> ActualizarAsync(DocenteDepartamentoCrear p) =>
        (await _bd.PrimeroAsync<DocenteDepartamento>("sp_actualizar_docente_departamento", new
        {
            docente = p.Docente,
            departamento = p.Departamento,
            dedicacion = p.Dedicacion,
            modalidad = p.Modalidad,
            fecha_ingreso = p.FechaIngreso,
            fecha_salida = p.FechaSalida
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int docente, int departamento) =>
        _bd.EjecutarAsync("sp_eliminar_docente_departamento", new { docente, departamento });
}
