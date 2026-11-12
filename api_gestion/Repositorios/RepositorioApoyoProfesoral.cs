using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>apoyo_profesoral</c>.</summary>
public interface IRepositorioApoyoProfesoral
{
    Task<List<ApoyoProfesoral>> ListarAsync();
    Task<ApoyoProfesoral?> ObtenerAsync(int estudios);
    Task<ApoyoProfesoral> CrearAsync(ApoyoProfesoralCrear p);
    Task<ApoyoProfesoral> ActualizarAsync(ApoyoProfesoralCrear p);
    Task EliminarAsync(int estudios);
}

/// <summary>
/// Repositorio (capa de datos) de <c>apoyo_profesoral (DETALLE 1:1 de estudios_realizados)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioApoyoProfesoral : IRepositorioApoyoProfesoral
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioApoyoProfesoral(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<ApoyoProfesoral>> ListarAsync() =>
        _bd.ListarAsync<ApoyoProfesoral>("sp_listar_apoyo_profesoral");

    public Task<ApoyoProfesoral?> ObtenerAsync(int estudios) =>
        _bd.PrimeroAsync<ApoyoProfesoral>("sp_consultar_apoyo_profesoral", new { estudios });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<ApoyoProfesoral> CrearAsync(ApoyoProfesoralCrear p) =>
        (await _bd.PrimeroAsync<ApoyoProfesoral>("sp_crear_apoyo_profesoral", new
        {
            estudios = p.Estudios,
            con_apoyo = p.ConApoyo,
            institucion = p.Institucion,
            tipo = p.Tipo
        }))!;

    public async Task<ApoyoProfesoral> ActualizarAsync(ApoyoProfesoralCrear p) =>
        (await _bd.PrimeroAsync<ApoyoProfesoral>("sp_actualizar_apoyo_profesoral", new
        {
            estudios = p.Estudios,
            con_apoyo = p.ConApoyo,
            institucion = p.Institucion,
            tipo = p.Tipo
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int estudios) =>
        _bd.EjecutarAsync("sp_eliminar_apoyo_profesoral", new { estudios });
}
