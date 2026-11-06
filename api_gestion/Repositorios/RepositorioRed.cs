using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>red</c>.</summary>
public interface IRepositorioRed
{
    Task<List<Red>> ListarAsync();
    Task<Red?> ObtenerAsync(int clave);
    Task<Red> CrearAsync(Red entidad);
    Task<Red> ActualizarAsync(Red entidad);
    Task EliminarLogicoAsync(int clave);
}

/// <summary>
/// Repositorio (capa de datos) de <c>red</c>.
/// Desde la v2 no tiene SQL: cada método llama a un procedimiento almacenado.
/// </summary>
public class RepositorioRed : IRepositorioRed
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioRed(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Red>> ListarAsync() =>
        _bd.ListarAsync<Red>("sp_listar_red");

    public Task<Red?> ObtenerAsync(int clave) =>
        _bd.PrimeroAsync<Red>("sp_consultar_red", new { idr = clave });

    public async Task<Red> CrearAsync(Red entidad) =>
        (await _bd.PrimeroAsync<Red>("sp_crear_red",
            new { idr = entidad.Idr, nombre = entidad.Nombre, url = entidad.Url, pais = entidad.Pais }))!;

    public async Task<Red> ActualizarAsync(Red entidad) =>
        (await _bd.PrimeroAsync<Red>("sp_actualizar_red",
            new { idr = entidad.Idr, nombre = entidad.Nombre, url = entidad.Url, pais = entidad.Pais }))!;

    // Borrado LÓGICO (activo = 0).
    public Task EliminarLogicoAsync(int clave) =>
        _bd.EjecutarAsync("sp_eliminar_red", new { idr = clave });
}
