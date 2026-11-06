using System.Text.Json;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>reconocimiento</c>.</summary>
public interface IRepositorioReconocimiento
{
    Task<List<Reconocimiento>> ListarAsync();
    Task<List<Reconocimiento>> ListarPorDocenteAsync(int cedula);
    Task<Reconocimiento?> ObtenerAsync(int id);
    Task<Reconocimiento> CrearAsync(ReconocimientoCrear p);
    Task<List<Reconocimiento>> CrearLoteAsync(int cedula, List<ReconocimientoRenglon> renglones);
    Task<Reconocimiento> ActualizarAsync(int id, ReconocimientoCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// Repositorio (capa de datos) de <c>reconocimiento (DETALLE de docente)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioReconocimiento : IRepositorioReconocimiento
{
    private readonly IEjecutorProcedimientos _bd;

    // Los renglones se mandan en snake_case, igual que los nombres que lee OPENJSON en el procedimiento.
    private static readonly JsonSerializerOptions JsonSnakeCase = new() { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower };

    public RepositorioReconocimiento(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Reconocimiento>> ListarAsync() =>
        _bd.ListarAsync<Reconocimiento>("sp_listar_reconocimiento");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<Reconocimiento>> ListarPorDocenteAsync(int cedula) =>
        _bd.ListarAsync<Reconocimiento>("sp_listar_reconocimiento", new { docente = cedula });

    public Task<Reconocimiento?> ObtenerAsync(int id) =>
        _bd.PrimeroAsync<Reconocimiento>("sp_consultar_reconocimiento", new { id });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<Reconocimiento> CrearAsync(ReconocimientoCrear p) =>
        (await _bd.PrimeroAsync<Reconocimiento>("sp_crear_reconocimiento", new
        {
            tipo = p.Tipo,
            fecha = p.Fecha,
            institucion = p.Institucion,
            nombre = p.Nombre,
            ambito = p.Ambito,
            docente = p.Docente
        }))!;

    // Envío múltiple: TODOS los renglones viajan en un solo parámetro JSON y el
    // procedimiento los guarda en una sola transacción (todos o ninguno).
    public Task<List<Reconocimiento>> CrearLoteAsync(int cedula, List<ReconocimientoRenglon> renglones) =>
        _bd.ListarAsync<Reconocimiento>("sp_crear_reconocimiento_lote", new
        {
            docente = cedula,
            json = JsonSerializer.Serialize(renglones, JsonSnakeCase)
        });

    public async Task<Reconocimiento> ActualizarAsync(int id, ReconocimientoCrear p) =>
        (await _bd.PrimeroAsync<Reconocimiento>("sp_actualizar_reconocimiento", new
        {
            id,
            tipo = p.Tipo,
            fecha = p.Fecha,
            institucion = p.Institucion,
            nombre = p.Nombre,
            ambito = p.Ambito,
            docente = p.Docente
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int id) =>
        _bd.EjecutarAsync("sp_eliminar_reconocimiento", new { id });
}
