using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>beca</c>.</summary>
public interface IRepositorioBeca
{
    Task<List<Beca>> ListarAsync();
    Task<Beca?> ObtenerAsync(int estudios);
    Task<Beca> CrearAsync(BecaCrear p);
    Task<Beca> ActualizarAsync(BecaCrear p);
    Task EliminarAsync(int estudios);
}

/// <summary>
/// Repositorio (capa de datos) de <c>beca (DETALLE 1:1 de estudios_realizados)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioBeca : IRepositorioBeca
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioBeca(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Beca>> ListarAsync() =>
        _bd.ListarAsync<Beca>("sp_listar_beca");

    public Task<Beca?> ObtenerAsync(int estudios) =>
        _bd.PrimeroAsync<Beca>("sp_consultar_beca", new { estudios });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<Beca> CrearAsync(BecaCrear p) =>
        (await _bd.PrimeroAsync<Beca>("sp_crear_beca", new
        {
            estudios = p.Estudios,
            tipo = p.Tipo,
            institucion = p.Institucion,
            fecha_inicio = p.FechaInicio,
            fecha_fin = p.FechaFin
        }))!;

    public async Task<Beca> ActualizarAsync(BecaCrear p) =>
        (await _bd.PrimeroAsync<Beca>("sp_actualizar_beca", new
        {
            estudios = p.Estudios,
            tipo = p.Tipo,
            institucion = p.Institucion,
            fecha_inicio = p.FechaInicio,
            fecha_fin = p.FechaFin
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int estudios) =>
        _bd.EjecutarAsync("sp_eliminar_beca", new { estudios });
}
