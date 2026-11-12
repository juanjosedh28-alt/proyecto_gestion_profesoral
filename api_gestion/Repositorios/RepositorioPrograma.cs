using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>programa</c>.</summary>
public interface IRepositorioPrograma
{
    Task<List<Programa>> ListarAsync();
    Task<Programa?> ObtenerAsync(int clave);
    Task<Programa> CrearAsync(Programa entidad);
    Task<Programa> ActualizarAsync(Programa entidad);
    Task EliminarLogicoAsync(int clave);
}

/// <summary>
/// Repositorio (capa de datos) de <c>programa</c>.
/// Desde la v2 no tiene SQL: cada método llama a un procedimiento almacenado.
/// La fecha de actualización la pone el procedimiento con la fecha de hoy.
/// </summary>
public class RepositorioPrograma : IRepositorioPrograma
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioPrograma(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Programa>> ListarAsync() =>
        _bd.ListarAsync<Programa>("sp_listar_programa");

    public Task<Programa?> ObtenerAsync(int clave) =>
        _bd.PrimeroAsync<Programa>("sp_consultar_programa", new { id = clave });

    public async Task<Programa> CrearAsync(Programa entidad) =>
        (await _bd.PrimeroAsync<Programa>("sp_crear_programa", Parametros(entidad)))!;

    public async Task<Programa> ActualizarAsync(Programa entidad) =>
        (await _bd.PrimeroAsync<Programa>("sp_actualizar_programa", Parametros(entidad)))!;

    // Borrado LÓGICO (activo = 0).
    public Task EliminarLogicoAsync(int clave) =>
        _bd.EjecutarAsync("sp_eliminar_programa", new { id = clave });

    // Crear y actualizar reciben los mismos parámetros: se arman en un solo sitio.
    private static object Parametros(Programa e) => new
    {
        id = e.Id,
        nombre = e.Nombre,
        tipo = e.Tipo,
        nivel = e.Nivel,
        fecha_creacion = e.FechaCreacion,
        fecha_cierre = e.FechaCierre,
        numero_cohortes = e.NumeroCohortes,
        cant_graduados = e.CantGraduados,
        ciudad = e.Ciudad,
        facultad = e.Facultad
    };
}
