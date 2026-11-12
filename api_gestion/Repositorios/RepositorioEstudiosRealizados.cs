using System.Text.Json;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>estudios_realizados</c>.</summary>
public interface IRepositorioEstudiosRealizados
{
    Task<List<EstudiosRealizados>> ListarAsync();
    Task<List<EstudiosRealizados>> ListarPorDocenteAsync(int cedula);
    Task<EstudiosRealizados?> ObtenerAsync(int id);
    Task<EstudiosRealizados> CrearAsync(EstudiosRealizadosCrear p);
    Task<List<EstudiosRealizados>> CrearLoteAsync(int cedula, List<EstudiosRealizadosRenglon> renglones);
    Task<EstudiosRealizados> ActualizarAsync(EstudiosRealizadosCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Repositorio (capa de datos) de <c>estudios_realizados (DETALLE de docente)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioEstudiosRealizados : IRepositorioEstudiosRealizados
{
    private readonly IEjecutorProcedimientos _bd;

    // Los renglones se mandan en snake_case, igual que los nombres que lee OPENJSON en el procedimiento.
    private static readonly JsonSerializerOptions JsonSnakeCase = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public RepositorioEstudiosRealizados(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<EstudiosRealizados>> ListarAsync() =>
        _bd.ListarAsync<EstudiosRealizados>("sp_listar_estudios_realizados");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<EstudiosRealizados>> ListarPorDocenteAsync(int cedula) =>
        _bd.ListarAsync<EstudiosRealizados>("sp_listar_estudios_realizados", new { docente = cedula });

    public Task<EstudiosRealizados?> ObtenerAsync(int id) =>
        _bd.PrimeroAsync<EstudiosRealizados>("sp_consultar_estudios_realizados", new { id });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<EstudiosRealizados> CrearAsync(EstudiosRealizadosCrear p) =>
        (await _bd.PrimeroAsync<EstudiosRealizados>("sp_crear_estudios_realizados", new
        {
            id = p.Id,
            titulo = p.Titulo,
            universidad = p.Universidad,
            fecha = p.Fecha,
            tipo = p.Tipo,
            ciudad = p.Ciudad,
            ins_acreditada = p.InsAcreditada,
            metodologia = p.Metodologia,
            perfil_egresado = p.PerfilEgresado,
            pais = p.Pais,
            docente = p.Docente
        }))!;

    // Envío múltiple: TODOS los renglones viajan en un solo parámetro JSON y el
    // procedimiento los guarda en una sola transacción (todos o ninguno).
    public Task<List<EstudiosRealizados>> CrearLoteAsync(int cedula, List<EstudiosRealizadosRenglon> renglones) =>
        _bd.ListarAsync<EstudiosRealizados>("sp_crear_estudios_realizados_lote", new
        {
            docente = cedula,
            json = JsonSerializer.Serialize(renglones, JsonSnakeCase)
        });

    public async Task<EstudiosRealizados> ActualizarAsync(EstudiosRealizadosCrear p) =>
        (await _bd.PrimeroAsync<EstudiosRealizados>("sp_actualizar_estudios_realizados", new
        {
            id = p.Id,
            titulo = p.Titulo,
            universidad = p.Universidad,
            fecha = p.Fecha,
            tipo = p.Tipo,
            ciudad = p.Ciudad,
            ins_acreditada = p.InsAcreditada,
            metodologia = p.Metodologia,
            perfil_egresado = p.PerfilEgresado,
            pais = p.Pais,
            docente = p.Docente
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int id) =>
        _bd.EjecutarAsync("sp_eliminar_estudios_realizados", new { id });
}
