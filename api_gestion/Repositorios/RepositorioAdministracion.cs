using System.Text.Json;
using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

// ===================================================================
// (v3) Repositorios de la ADMINISTRACIÓN del acceso: usuario, rol_usuario,
// rol, ruta y ruta_rol. Igual que en la v2: ninguna consulta escrita aquí,
// cada método es una llamada a un procedimiento de db/v3_control_acceso.sql.
// ===================================================================

/// <summary>Contrato de acceso a datos de <c>usuario</c> y <c>rol_usuario</c>.</summary>
public interface IRepositorioUsuario
{
    Task<List<Usuario>> ListarAsync();
    Task<Usuario?> ObtenerAsync(int id);
    Task<Usuario> CrearAsync(string username, string hash, string email, string? nombreCompleto, List<int> rolIds);
    Task<Usuario> ActualizarAsync(int id, string username, string? hash, string email, string? nombreCompleto, List<int> rolIds);
    Task EliminarAsync(int id);
    Task<List<RolUsuario>> RolesAsync(int usuarioId);
    Task<RolUsuario> AsignarRolAsync(int usuarioId, int rolId);
    Task QuitarRolAsync(int usuarioId, int rolId);
}

/// <summary>
/// Repositorio de usuarios. Recibe el HASH ya calculado por el servicio: la
/// contraseña en claro no pasa por aquí ni llega a la base. Los roles viajan
/// como lista JSON en un solo parámetro para que usuario y roles se guarden
/// en una sola transacción.
/// </summary>
public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioUsuario(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Usuario>> ListarAsync() => _bd.ListarAsync<Usuario>("sp_listar_usuario");

    public Task<Usuario?> ObtenerAsync(int id) => _bd.PrimeroAsync<Usuario>("sp_consultar_usuario", new { id });

    public async Task<Usuario> CrearAsync(string username, string hash, string email, string? nombreCompleto, List<int> rolIds) =>
        (await _bd.PrimeroAsync<Usuario>("sp_crear_usuario", new
        {
            username,
            password_hash = hash,
            email,
            nombre_completo = nombreCompleto,
            roles_json = JsonSerializer.Serialize(rolIds)
        }))!;

    // hash null = la contraseña no cambia (el procedimiento usa ISNULL).
    public async Task<Usuario> ActualizarAsync(int id, string username, string? hash, string email, string? nombreCompleto, List<int> rolIds) =>
        (await _bd.PrimeroAsync<Usuario>("sp_actualizar_usuario", new
        {
            id,
            username,
            password_hash = hash,
            email,
            nombre_completo = nombreCompleto,
            roles_json = JsonSerializer.Serialize(rolIds)
        }))!;

    // Borrado LÓGICO: el procedimiento marca activo = 0.
    public Task EliminarAsync(int id) => _bd.EjecutarAsync("sp_eliminar_usuario", new { id });

    public Task<List<RolUsuario>> RolesAsync(int usuarioId) =>
        _bd.ListarAsync<RolUsuario>("sp_listar_rol_usuario", new { usuario_id = usuarioId });

    public async Task<RolUsuario> AsignarRolAsync(int usuarioId, int rolId) =>
        (await _bd.PrimeroAsync<RolUsuario>("sp_crear_rol_usuario", new { usuario_id = usuarioId, rol_id = rolId }))!;

    public Task QuitarRolAsync(int usuarioId, int rolId) =>
        _bd.EjecutarAsync("sp_eliminar_rol_usuario", new { usuario_id = usuarioId, rol_id = rolId });
}

/// <summary>Contrato de acceso a datos de <c>rol</c>.</summary>
public interface IRepositorioRol
{
    Task<List<Rol>> ListarAsync();
    Task<Rol?> ObtenerAsync(int id);
    Task<Rol> CrearAsync(string nombre, string? descripcion);
    Task<Rol> ActualizarAsync(int id, string nombre, string? descripcion);
    Task EliminarAsync(int id);
}

/// <summary>Repositorio de roles: cada método llama a su procedimiento.</summary>
public class RepositorioRol : IRepositorioRol
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioRol(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Rol>> ListarAsync() => _bd.ListarAsync<Rol>("sp_listar_rol");

    public Task<Rol?> ObtenerAsync(int id) => _bd.PrimeroAsync<Rol>("sp_consultar_rol", new { id });

    public async Task<Rol> CrearAsync(string nombre, string? descripcion) =>
        (await _bd.PrimeroAsync<Rol>("sp_crear_rol", new { nombre, descripcion }))!;

    public async Task<Rol> ActualizarAsync(int id, string nombre, string? descripcion) =>
        (await _bd.PrimeroAsync<Rol>("sp_actualizar_rol", new { id, nombre, descripcion }))!;

    // Borrado LÓGICO. Bloqueado si es Administrador o si hay usuarios activos con el rol.
    public Task EliminarAsync(int id) => _bd.EjecutarAsync("sp_eliminar_rol", new { id });
}

/// <summary>Contrato de acceso a datos de <c>ruta</c> y <c>ruta_rol</c> (los permisos).</summary>
public interface IRepositorioPermiso
{
    Task<List<Ruta>> RutasAsync();
    Task<List<PermisoDeRol>> PermisosDeRolAsync(int rolId);
    Task DarAsync(int rutaId, int rolId);
    Task QuitarAsync(int rutaId, int rolId);
}

/// <summary>
/// Repositorio de permisos. Quitar un permiso borra la fila de ruta_rol, y
/// como sp_verificar_acceso_ruta se consulta en cada petición, el cambio
/// surte efecto de inmediato para todos los usuarios con ese rol.
/// </summary>
public class RepositorioPermiso : IRepositorioPermiso
{
    private readonly IEjecutorProcedimientos _bd;

    public RepositorioPermiso(IEjecutorProcedimientos bd) => _bd = bd;

    public Task<List<Ruta>> RutasAsync() => _bd.ListarAsync<Ruta>("sp_listar_ruta");

    public Task<List<PermisoDeRol>> PermisosDeRolAsync(int rolId) =>
        _bd.ListarAsync<PermisoDeRol>("sp_listar_ruta_rol", new { rol_id = rolId });

    public Task DarAsync(int rutaId, int rolId) =>
        _bd.EjecutarAsync("sp_crear_ruta_rol", new { ruta_id = rutaId, rol_id = rolId });

    public Task QuitarAsync(int rutaId, int rolId) =>
        _bd.EjecutarAsync("sp_eliminar_ruta_rol", new { ruta_id = rutaId, rol_id = rolId });
}
