using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>intereses_futuros</c>.</summary>
public interface IRepositorioInteresesFuturos
{
    Task<List<InteresesFuturos>> ListarAsync();
    Task<List<InteresesFuturos>> ListarPorDocenteAsync(int cedula);
    Task<InteresesFuturos?> ObtenerAsync(int docente, string terminoClave);
    Task<InteresesFuturos> CrearAsync(InteresesFuturosCrear p);
    Task EliminarAsync(int docente, string terminoClave);
}

/// <summary>
/// Repositorio (capa de datos) de <c>intereses_futuros (PUENTE PURO docente <-> termino_clave)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioInteresesFuturos : IRepositorioInteresesFuturos
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioInteresesFuturos(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<InteresesFuturos>> ListarAsync() =>
        _bd.ListarAsync<InteresesFuturos>("sp_listar_intereses_futuros");

    // El mismo procedimiento de listar, pero filtrando por el maestro.
    public Task<List<InteresesFuturos>> ListarPorDocenteAsync(int cedula) =>
        _bd.ListarAsync<InteresesFuturos>("sp_listar_intereses_futuros", new { docente = cedula });

    public Task<InteresesFuturos?> ObtenerAsync(int docente, string terminoClave) =>
        _bd.PrimeroAsync<InteresesFuturos>("sp_consultar_intereses_futuros", new { docente, termino_clave = terminoClave });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<InteresesFuturos> CrearAsync(InteresesFuturosCrear p) =>
        (await _bd.PrimeroAsync<InteresesFuturos>("sp_crear_intereses_futuros", new
        {
            docente = p.Docente,
            termino_clave = p.TerminoClave
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int docente, string terminoClave) =>
        _bd.EjecutarAsync("sp_eliminar_intereses_futuros", new { docente, termino_clave = terminoClave });
}
