using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>estudio_ac</c>.</summary>
public interface IRepositorioEstudioAc
{
    Task<List<EstudioAc>> ListarAsync();
    Task<List<EstudioAc>> ListarPorEstudioAsync(int id);
    Task<EstudioAc?> ObtenerAsync(int estudio, string areaConocimiento);
    Task<EstudioAc> CrearAsync(EstudioAcCrear p);
    Task EliminarAsync(int estudio, string areaConocimiento);
}

/// <summary>
/// Repositorio (capa de datos) de <c>estudio_ac (PUENTE PURO estudios_realizados <-> area_conocimiento)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioEstudioAc : IRepositorioEstudioAc
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioEstudioAc(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<EstudioAc>> ListarAsync() =>
        _bd.ListarAsync<EstudioAc>("sp_listar_estudio_ac");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<EstudioAc>> ListarPorEstudioAsync(int id) =>
        _bd.ListarAsync<EstudioAc>("sp_listar_estudio_ac", new { estudio = id });

    public Task<EstudioAc?> ObtenerAsync(int estudio, string areaConocimiento) =>
        _bd.PrimeroAsync<EstudioAc>("sp_consultar_estudio_ac", new { estudio, area_conocimiento = areaConocimiento });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<EstudioAc> CrearAsync(EstudioAcCrear p) =>
        (await _bd.PrimeroAsync<EstudioAc>("sp_crear_estudio_ac", new
        {
            estudio = p.Estudio,
            area_conocimiento = p.AreaConocimiento
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int estudio, string areaConocimiento) =>
        _bd.EjecutarAsync("sp_eliminar_estudio_ac", new { estudio, area_conocimiento = areaConocimiento });
}
