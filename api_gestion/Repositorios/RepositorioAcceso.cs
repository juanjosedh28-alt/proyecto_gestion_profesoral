using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato del acceso a datos del control de acceso (v3).</summary>
public interface IRepositorioAcceso
{
    /// <summary>¿Ese correo puede entrar a esa ruta? Lo usa ExigePermiso en cada petición.</summary>
    Task<bool> TieneAccesoAsync(string email, string ruta);

    /// <summary>Las rutas de ese correo, para armar el menú.</summary>
    Task<List<string>> RutasPermitidasAsync(string email);

    /// <summary>El hash guardado de un correo activo, o null. Solo para iniciar sesión.</summary>
    Task<Credenciales?> CredencialesAsync(string email);

    /// <summary>Los nombres de los roles activos de ese correo.</summary>
    Task<List<string>> RolesAsync(string email);
}

/// <summary>
/// Repositorio del control de acceso. Igual que los de la v2: NO tiene SQL,
/// solo llama procedimientos. La decisión del permiso la toma
/// sp_verificar_acceso_ruta en la base; aquí no se escribe el cruce de tablas de los permisos.
/// </summary>
public class RepositorioAcceso : IRepositorioAcceso
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioAcceso(IEjecutorProcedimientos bd) => _bd = bd;

    public async Task<bool> TieneAccesoAsync(string email, string ruta) =>
        await _bd.PrimeroAsync<bool>("sp_verificar_acceso_ruta", new { email, ruta });

    public Task<List<string>> RutasPermitidasAsync(string email) =>
        _bd.ListarAsync<string>("sp_rutas_permitidas", new { email });

    public Task<Credenciales?> CredencialesAsync(string email) =>
        _bd.PrimeroAsync<Credenciales>("sp_credenciales_usuario", new { email });

    public Task<List<string>> RolesAsync(string email) =>
        _bd.ListarAsync<string>("sp_roles_de_usuario", new { email });
}
