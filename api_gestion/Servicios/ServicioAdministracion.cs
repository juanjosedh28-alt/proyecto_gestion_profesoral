using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;
using BC = BCrypt.Net.BCrypt;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de los usuarios (v3).</summary>
public interface IServicioUsuario
{
    Task<List<Usuario>> ListarAsync();
    Task<Usuario> ObtenerAsync(int id);
    Task<Usuario> CrearAsync(UsuarioCrear p);
    Task<Usuario> ActualizarAsync(int id, UsuarioCrear p);
    Task EliminarAsync(int id, string? quienPide);
    Task<List<RolUsuario>> RolesAsync(int usuarioId);
    Task<RolUsuario> AsignarRolAsync(RolUsuarioCrear p);
    Task QuitarRolAsync(int usuarioId, int rolId);
}

/// <summary>
/// (v3) Servicio de usuarios. Su trabajo especial es la CONTRASEÑA:
/// - Al crear: obligatoria, mínimo 8 caracteres, y se guarda su HASH bcrypt
///   (costo 12). La contraseña en claro no sale de este método.
/// - Al editar: vacía = no cambia (se manda null y la base conserva el hash).
/// - Ninguna respuesta lleva la contraseña ni el hash.
/// Además no deja que un administrador se retire a sí mismo: se quedaría
/// por fuera en la siguiente petición.
/// </summary>
public class ServicioUsuario : IServicioUsuario
{
    private const int CostoBcrypt = 12;   // subirlo a 13 duplica el tiempo: así se encarece cuando las máquinas sean más rápidas
    private const int MinimoContrasena = 8;

    private readonly IRepositorioUsuario _repo;

    public ServicioUsuario(IRepositorioUsuario repo) => _repo = repo;

    public Task<List<Usuario>> ListarAsync() => _repo.ListarAsync();

    public async Task<Usuario> ObtenerAsync(int id) =>
        await _repo.ObtenerAsync(id) ?? throw new NoEncontradoExcepcion($"No se encontró el usuario con id {id}.");

    public Task<Usuario> CrearAsync(UsuarioCrear p)
    {
        if (string.IsNullOrWhiteSpace(p.Contrasena))
            throw new ValidacionExcepcion("Datos inválidos.", "contrasena: La contraseña es obligatoria al crear un usuario.");
        RevisarContrasena(p.Contrasena);

        var hash = BC.HashPassword(p.Contrasena, CostoBcrypt);
        return _repo.CrearAsync(p.Username.Trim(), hash, p.Email.Trim(), p.NombreCompleto, p.RolIds);
    }

    public Task<Usuario> ActualizarAsync(int id, UsuarioCrear p)
    {
        string? hash = null;   // null = la contraseña se queda como está
        if (!string.IsNullOrWhiteSpace(p.Contrasena))
        {
            RevisarContrasena(p.Contrasena);
            hash = BC.HashPassword(p.Contrasena, CostoBcrypt);
        }
        return _repo.ActualizarAsync(id, p.Username.Trim(), hash, p.Email.Trim(), p.NombreCompleto, p.RolIds);
    }

    public async Task EliminarAsync(int id, string? quienPide)
    {
        var usuario = await ObtenerAsync(id);
        if (string.Equals(usuario.Email, quienPide, StringComparison.OrdinalIgnoreCase))
            throw new ConflictoExcepcion("No puede retirar su propio usuario.");
        await _repo.EliminarAsync(id);
    }

    public Task<List<RolUsuario>> RolesAsync(int usuarioId) => _repo.RolesAsync(usuarioId);

    public Task<RolUsuario> AsignarRolAsync(RolUsuarioCrear p) => _repo.AsignarRolAsync(p.UsuarioId, p.RolId);

    public Task QuitarRolAsync(int usuarioId, int rolId) => _repo.QuitarRolAsync(usuarioId, rolId);

    private static void RevisarContrasena(string contrasena)
    {
        if (contrasena.Length < MinimoContrasena)
            throw new ValidacionExcepcion("Datos inválidos.", $"contrasena: La contraseña debe tener al menos {MinimoContrasena} caracteres.");
    }
}

/// <summary>Contrato de las reglas de negocio de los roles (v3).</summary>
public interface IServicioRol
{
    Task<List<Rol>> ListarAsync();
    Task<Rol> ObtenerAsync(int id);
    Task<Rol> CrearAsync(RolCrear p);
    Task<Rol> ActualizarAsync(int id, RolCrear p);
    Task EliminarAsync(int id);
}

/// <summary>
/// (v3) Servicio de roles. Las reglas (nombre repetido, no retirar el
/// Administrador ni un rol que todavía tenga usuarios) las revisa el
/// procedimiento, como en la v2.
/// </summary>
public class ServicioRol : IServicioRol
{
    private readonly IRepositorioRol _repo;

    public ServicioRol(IRepositorioRol repo) => _repo = repo;

    public Task<List<Rol>> ListarAsync() => _repo.ListarAsync();

    public async Task<Rol> ObtenerAsync(int id) =>
        await _repo.ObtenerAsync(id) ?? throw new NoEncontradoExcepcion($"No se encontró el rol con id {id}.");

    public Task<Rol> CrearAsync(RolCrear p) => _repo.CrearAsync(p.Nombre.Trim(), p.Descripcion);

    public Task<Rol> ActualizarAsync(int id, RolCrear p) => _repo.ActualizarAsync(id, p.Nombre.Trim(), p.Descripcion);

    public Task EliminarAsync(int id) => _repo.EliminarAsync(id);
}
