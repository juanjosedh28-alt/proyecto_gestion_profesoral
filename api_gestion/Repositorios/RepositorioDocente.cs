using ApiGestion.Modelos;
using ApiGestion.Peticiones;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>docente</c>.</summary>
public interface IRepositorioDocente
{
    Task<List<Docente>> ListarAsync();
    Task<Docente?> ObtenerAsync(int cedula);
    Task<Docente> CrearAsync(DocenteCrear p);
    Task<Docente> ActualizarAsync(DocenteCrear p);
    Task EliminarAsync(int cedula);
}

/// <summary>
/// Repositorio (capa de datos) de <c>docente (MAESTRO del módulo)</c>.
/// No tiene SQL: cada método es UNA llamada a un procedimiento almacenado de
/// db/v2_procedimientos.sql con sus parámetros. Las reglas (¿existe el padre?,
/// ¿está repetido?) las revisa el procedimiento; si una falla, la excepción
/// sube y el middleware la convierte en 404/409/422.
/// </summary>
public class RepositorioDocente : IRepositorioDocente
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioDocente(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Docente>> ListarAsync() =>
        _bd.ListarAsync<Docente>("sp_listar_docente");

    public Task<Docente?> ObtenerAsync(int cedula) =>
        _bd.PrimeroAsync<Docente>("sp_consultar_docente", new { cedula });

    // El procedimiento devuelve la fila COMO QUEDÓ GUARDADA (con lo que haya puesto la base).
    public async Task<Docente> CrearAsync(DocenteCrear p) =>
        (await _bd.PrimeroAsync<Docente>("sp_crear_docente", new
        {
            cedula = p.Cedula,
            nombres = p.Nombres,
            apellidos = p.Apellidos,
            genero = p.Genero,
            cargo = p.Cargo,
            fecha_nacimiento = p.FechaNacimiento,
            correo = p.Correo,
            telefono = p.Telefono,
            url_cvlac = p.UrlCvlac,
            escalafon = p.Escalafon,
            perfil = p.Perfil,
            cat_minciencia = p.CatMinciencia,
            conv_minciencia = p.ConvMinciencia,
            nacionalidaad = p.Nacionalidaad,
            linea_investigacion_principal = p.LineaInvestigacionPrincipal
        }))!;

    public async Task<Docente> ActualizarAsync(DocenteCrear p) =>
        (await _bd.PrimeroAsync<Docente>("sp_actualizar_docente", new
        {
            cedula = p.Cedula,
            nombres = p.Nombres,
            apellidos = p.Apellidos,
            genero = p.Genero,
            cargo = p.Cargo,
            fecha_nacimiento = p.FechaNacimiento,
            correo = p.Correo,
            telefono = p.Telefono,
            url_cvlac = p.UrlCvlac,
            escalafon = p.Escalafon,
            perfil = p.Perfil,
            cat_minciencia = p.CatMinciencia,
            conv_minciencia = p.ConvMinciencia,
            nacionalidaad = p.Nacionalidaad,
            linea_investigacion_principal = p.LineaInvestigacionPrincipal
        }))!;

    // Borrado LÓGICO: el procedimiento solo marca la fila con activo = 0.
    public Task EliminarAsync(int cedula) =>
        _bd.EjecutarAsync("sp_eliminar_docente", new { cedula });
}
