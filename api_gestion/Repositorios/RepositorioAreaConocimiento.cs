using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>area_conocimiento</c>.</summary>
public interface IRepositorioAreaConocimiento
{
    Task<List<AreaConocimiento>> ListarAsync();
    Task<AreaConocimiento?> ObtenerAsync(string clave);
}

/// <summary>
/// Repositorio (capa de datos) de <c>area_conocimiento</c>, catálogo de solo lectura.
/// Desde la v2 no tiene SQL: llama a los procedimientos de db/v2_procedimientos.sql
/// (que filtran <c>activo = 1</c>).
/// </summary>
public class RepositorioAreaConocimiento : IRepositorioAreaConocimiento
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioAreaConocimiento(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<AreaConocimiento>> ListarAsync() =>
        _bd.ListarAsync<AreaConocimiento>("sp_listar_area_conocimiento");

    public Task<AreaConocimiento?> ObtenerAsync(string clave) =>
        _bd.PrimeroAsync<AreaConocimiento>("sp_consultar_area_conocimiento", new { id = clave });
}
