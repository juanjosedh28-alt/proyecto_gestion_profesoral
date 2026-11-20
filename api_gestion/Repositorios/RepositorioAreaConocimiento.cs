using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>area_conocimiento</c>.</summary>
public interface IRepositorioAreaConocimiento
{
    Task<List<AreaConocimiento>> ListarAsync();
    Task<AreaConocimiento?> ObtenerAsync(string clave);
    Task<AreaConocimiento> CrearAsync(AreaConocimiento entidad);
    Task<AreaConocimiento> ActualizarAsync(AreaConocimiento entidad);
    Task EliminarLogicoAsync(string clave);
}

/// <summary>
/// Repositorio (capa de datos) de <c>area_conocimiento</c>.
/// No tiene SQL: cada método llama a un procedimiento de db/v2_procedimientos.sql.
/// Los listados filtran <c>activo = 1</c>; crear revisa el código repetido (409)
/// y actualizar o retirar revisan que exista (404).
/// </summary>
public class RepositorioAreaConocimiento : IRepositorioAreaConocimiento
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioAreaConocimiento(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<AreaConocimiento>> ListarAsync() =>
        _bd.ListarAsync<AreaConocimiento>("sp_listar_area_conocimiento");

    public Task<AreaConocimiento?> ObtenerAsync(string clave) =>
        _bd.PrimeroAsync<AreaConocimiento>("sp_consultar_area_conocimiento", new { id = clave });

    public async Task<AreaConocimiento> CrearAsync(AreaConocimiento entidad) =>
        (await _bd.PrimeroAsync<AreaConocimiento>("sp_crear_area_conocimiento", Parametros(entidad)))!;

    public async Task<AreaConocimiento> ActualizarAsync(AreaConocimiento entidad) =>
        (await _bd.PrimeroAsync<AreaConocimiento>("sp_actualizar_area_conocimiento", Parametros(entidad)))!;

    // Borrado LÓGICO (activo = 0).
    public Task EliminarLogicoAsync(string clave) =>
        _bd.EjecutarAsync("sp_eliminar_area_conocimiento", new { id = clave });

    private static object Parametros(AreaConocimiento e) =>
        new { id = e.Id, gran_area = e.GranArea, area = e.Area, disciplina = e.Disciplina };
}
