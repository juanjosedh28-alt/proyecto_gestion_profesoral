using System.Text.Json;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>experiecia</c>.</summary>
public interface IRepositorioExperiecia
{
    Task<List<Experiecia>> ListarAsync();
    Task<List<Experiecia>> ListarPorDocenteAsync(int cedula);
    Task<Experiecia?> ObtenerAsync(int id);
    Task<Experiecia> CrearAsync(ExperieciaCrear p);
    Task<List<Experiecia>> CrearLoteAsync(int cedula, List<ExperieciaRenglon> renglones);
    Task<Experiecia> ActualizarAsync(int id, ExperieciaCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Repositorio (capa de datos) de <c>experiecia (DETALLE de docente; se conserva la errata del esquema, regla R1)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioExperiecia : IRepositorioExperiecia
{
    private readonly IEjecutorProcedimientos _bd;

    // Los renglones se mandan en snake_case, igual que los nombres que lee OPENJSON en el procedimiento.
    private static readonly JsonSerializerOptions JsonSnakeCase = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public RepositorioExperiecia(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Experiecia>> ListarAsync() =>
        _bd.ListarAsync<Experiecia>("sp_listar_experiecia");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<Experiecia>> ListarPorDocenteAsync(int cedula) =>
        _bd.ListarAsync<Experiecia>("sp_listar_experiecia", new { docente = cedula });

    public Task<Experiecia?> ObtenerAsync(int id) =>
        _bd.PrimeroAsync<Experiecia>("sp_consultar_experiecia", new { id });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<Experiecia> CrearAsync(ExperieciaCrear p) =>
        (await _bd.PrimeroAsync<Experiecia>("sp_crear_experiecia", new
        {
            nombre_cargo = p.NombreCargo,
            institucion = p.Institucion,
            tipo = p.Tipo,
            fecha_inicio = p.FechaInicio,
            fecha_fin = p.FechaFin,
            docente = p.Docente
        }))!;

    // Envío múltiple: TODOS los renglones viajan en un solo parámetro JSON y el
    // procedimiento los guarda en una sola transacción (todos o ninguno).
    public Task<List<Experiecia>> CrearLoteAsync(int cedula, List<ExperieciaRenglon> renglones) =>
        _bd.ListarAsync<Experiecia>("sp_crear_experiecia_lote", new
        {
            docente = cedula,
            json = JsonSerializer.Serialize(renglones, JsonSnakeCase)
        });

    public async Task<Experiecia> ActualizarAsync(int id, ExperieciaCrear p) =>
        (await _bd.PrimeroAsync<Experiecia>("sp_actualizar_experiecia", new
        {
            id,
            nombre_cargo = p.NombreCargo,
            institucion = p.Institucion,
            tipo = p.Tipo,
            fecha_inicio = p.FechaInicio,
            fecha_fin = p.FechaFin,
            docente = p.Docente
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int id) =>
        _bd.EjecutarAsync("sp_eliminar_experiecia", new { id });
}
