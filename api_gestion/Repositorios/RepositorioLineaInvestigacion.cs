using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>linea_investigacion</c>.</summary>
public interface IRepositorioLineaInvestigacion
{
    Task<List<LineaInvestigacion>> ListarAsync();
    Task<LineaInvestigacion?> ObtenerAsync(int clave);
    Task<LineaInvestigacion> CrearAsync(LineaInvestigacion entidad);
    Task<LineaInvestigacion> ActualizarAsync(LineaInvestigacion entidad);
    Task EliminarLogicoAsync(int clave);
}

/// <summary>
/// Repositorio (capa de datos) de <c>linea_investigacion</c>.
/// Desde la v2 no tiene SQL: cada método llama a un procedimiento almacenado.
/// El id lo genera la base (IDENTITY) y el procedimiento devuelve la fila creada.
/// </summary>
public class RepositorioLineaInvestigacion : IRepositorioLineaInvestigacion
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioLineaInvestigacion(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<LineaInvestigacion>> ListarAsync() =>
        _bd.ListarAsync<LineaInvestigacion>("sp_listar_linea_investigacion");

    public Task<LineaInvestigacion?> ObtenerAsync(int clave) =>
        _bd.PrimeroAsync<LineaInvestigacion>("sp_consultar_linea_investigacion", new { id = clave });

    public async Task<LineaInvestigacion> CrearAsync(LineaInvestigacion entidad) =>
        (await _bd.PrimeroAsync<LineaInvestigacion>("sp_crear_linea_investigacion",
            new { nombre = entidad.Nombre, descripcion = entidad.Descripcion }))!;

    public async Task<LineaInvestigacion> ActualizarAsync(LineaInvestigacion entidad) =>
        (await _bd.PrimeroAsync<LineaInvestigacion>("sp_actualizar_linea_investigacion",
            new { id = entidad.Id, nombre = entidad.Nombre, descripcion = entidad.Descripcion }))!;

    // Borrado LÓGICO (activo = 0).
    public Task EliminarLogicoAsync(int clave) =>
        _bd.EjecutarAsync("sp_eliminar_linea_investigacion", new { id = clave });
}
