using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>red_docente</c>.</summary>
public interface IRepositorioRedDocente
{
    Task<List<RedDocente>> ListarAsync();
    Task<List<RedDocente>> ListarPorDocenteAsync(int cedula);
    Task<RedDocente?> ObtenerAsync(int red, int docente);
    Task<RedDocente> CrearAsync(RedDocenteCrear p);
    Task<RedDocente> ActualizarAsync(RedDocenteCrear p);
    Task EliminarAsync(int red, int docente);
}

/// <summary>
/// Repositorio (capa de datos) de <c>red_docente (PUENTE red <-> docente, con atributos)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioRedDocente : IRepositorioRedDocente
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioRedDocente(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<RedDocente>> ListarAsync() =>
        _bd.ListarAsync<RedDocente>("sp_listar_red_docente");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<RedDocente>> ListarPorDocenteAsync(int cedula) =>
        _bd.ListarAsync<RedDocente>("sp_listar_red_docente", new { docente = cedula });

    public Task<RedDocente?> ObtenerAsync(int red, int docente) =>
        _bd.PrimeroAsync<RedDocente>("sp_consultar_red_docente", new { red, docente });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<RedDocente> CrearAsync(RedDocenteCrear p) =>
        (await _bd.PrimeroAsync<RedDocente>("sp_crear_red_docente", new
        {
            red = p.Red,
            docente = p.Docente,
            fecha_inicio = p.FechaInicio,
            fecha_fin = p.FechaFin,
            act_destacadas = p.ActDestacadas
        }))!;

    public async Task<RedDocente> ActualizarAsync(RedDocenteCrear p) =>
        (await _bd.PrimeroAsync<RedDocente>("sp_actualizar_red_docente", new
        {
            red = p.Red,
            docente = p.Docente,
            fecha_inicio = p.FechaInicio,
            fecha_fin = p.FechaFin,
            act_destacadas = p.ActDestacadas
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int red, int docente) =>
        _bd.EjecutarAsync("sp_eliminar_red_docente", new { red, docente });
}
