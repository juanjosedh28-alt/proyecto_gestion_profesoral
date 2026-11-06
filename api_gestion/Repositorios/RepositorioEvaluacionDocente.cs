using System.Text.Json;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>evaluacion_docente</c>.</summary>
public interface IRepositorioEvaluacionDocente
{
    Task<List<EvaluacionDocente>> ListarAsync();
    Task<List<EvaluacionDocente>> ListarPorDocenteAsync(int cedula);
    Task<EvaluacionDocente?> ObtenerAsync(int id);
    Task<EvaluacionDocente> CrearAsync(EvaluacionDocenteCrear p);
    Task<List<EvaluacionDocente>> CrearLoteAsync(int cedula, List<EvaluacionDocenteRenglon> renglones);
    Task<EvaluacionDocente> ActualizarAsync(int id, EvaluacionDocenteCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Repositorio (capa de datos) de <c>evaluacion_docente (DETALLE de docente)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioEvaluacionDocente : IRepositorioEvaluacionDocente
{
    private readonly IEjecutorProcedimientos _bd;

    // Los renglones se mandan en snake_case, igual que los nombres que lee OPENJSON en el procedimiento.
    private static readonly JsonSerializerOptions JsonSnakeCase = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public RepositorioEvaluacionDocente(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<EvaluacionDocente>> ListarAsync() =>
        _bd.ListarAsync<EvaluacionDocente>("sp_listar_evaluacion_docente");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<EvaluacionDocente>> ListarPorDocenteAsync(int cedula) =>
        _bd.ListarAsync<EvaluacionDocente>("sp_listar_evaluacion_docente", new { docente = cedula });

    public Task<EvaluacionDocente?> ObtenerAsync(int id) =>
        _bd.PrimeroAsync<EvaluacionDocente>("sp_consultar_evaluacion_docente", new { id });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<EvaluacionDocente> CrearAsync(EvaluacionDocenteCrear p) =>
        (await _bd.PrimeroAsync<EvaluacionDocente>("sp_crear_evaluacion_docente", new
        {
            calificacion = p.Calificacion,
            semestre = p.Semestre,
            docente = p.Docente
        }))!;

    // Envío múltiple: TODOS los renglones viajan en un solo parámetro JSON y el
    // procedimiento los guarda en una sola transacción (todos o ninguno).
    public Task<List<EvaluacionDocente>> CrearLoteAsync(int cedula, List<EvaluacionDocenteRenglon> renglones) =>
        _bd.ListarAsync<EvaluacionDocente>("sp_crear_evaluacion_docente_lote", new
        {
            docente = cedula,
            json = JsonSerializer.Serialize(renglones, JsonSnakeCase)
        });

    public async Task<EvaluacionDocente> ActualizarAsync(int id, EvaluacionDocenteCrear p) =>
        (await _bd.PrimeroAsync<EvaluacionDocente>("sp_actualizar_evaluacion_docente", new
        {
            id,
            calificacion = p.Calificacion,
            semestre = p.Semestre,
            docente = p.Docente
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int id) =>
        _bd.EjecutarAsync("sp_eliminar_evaluacion_docente", new { id });
}
