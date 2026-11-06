using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>termino_clave</c>.</summary>
public interface IRepositorioTerminoClave
{
    Task<List<TerminoClave>> ListarAsync();
    Task<TerminoClave?> ObtenerAsync(string clave);
    Task<TerminoClave> CrearAsync(TerminoClave entidad);
    Task<TerminoClave> ActualizarAsync(TerminoClave entidad);
    Task EliminarLogicoAsync(string clave);
}

/// <summary>
/// Repositorio (capa de datos) de <c>termino_clave</c>.
/// Desde la v2 no tiene SQL: cada método llama a un procedimiento almacenado.
/// El procedimiento revisa si el término ya existe (409) o no se encuentra (404).
/// </summary>
public class RepositorioTerminoClave : IRepositorioTerminoClave
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioTerminoClave(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<TerminoClave>> ListarAsync() =>
        _bd.ListarAsync<TerminoClave>("sp_listar_termino_clave");

    public Task<TerminoClave?> ObtenerAsync(string clave) =>
        _bd.PrimeroAsync<TerminoClave>("sp_consultar_termino_clave", new { termino = clave });

    public async Task<TerminoClave> CrearAsync(TerminoClave entidad) =>
        (await _bd.PrimeroAsync<TerminoClave>("sp_crear_termino_clave",
            new { termino = entidad.Termino, termino_ingles = entidad.TerminoIngles }))!;

    public async Task<TerminoClave> ActualizarAsync(TerminoClave entidad) =>
        (await _bd.PrimeroAsync<TerminoClave>("sp_actualizar_termino_clave",
            new { termino = entidad.Termino, termino_ingles = entidad.TerminoIngles }))!;

    // Borrado LÓGICO (activo = 0).
    public Task EliminarLogicoAsync(string clave) =>
        _bd.EjecutarAsync("sp_eliminar_termino_clave", new { termino = clave });
}
