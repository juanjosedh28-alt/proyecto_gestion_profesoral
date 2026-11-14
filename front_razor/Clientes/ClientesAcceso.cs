using front_razor.Modelos;

namespace front_razor.Clientes;

// ===================================================================
// (v3) Clientes HTTP del control de acceso. El token lo pone ManejadorToken;
// aquí no se toca.
// ===================================================================

/// <summary>Inicio de sesión y permisos del usuario actual.</summary>
public class ClienteSesion : ClienteApiBase
{
    public ClienteSesion(HttpClient http) : base(http) { }

    /// <summary>
    /// POST /api/login. Si las credenciales no son correctas, la API responde
    /// 401 y aquí llega como ErrorApiException(401) con su mensaje: como esta
    /// petición no lleva token, no se confunde con una sesión vencida.
    /// </summary>
    public Task<SesionRespuesta> IniciarAsync(string email, string contrasena) =>
        PostAsync<SesionRespuesta>("api/login", new { email, contrasena });

    /// <summary>GET /api/permisos/mios: las rutas para armar el menú (la API saca el correo del token).</summary>
    public Task<MisPermisos> MisPermisosAsync() => GetAsync<MisPermisos>("api/permisos/mios");
}

/// <summary>Cliente HTTP de <c>/api/usuario</c> y <c>/api/rol-usuario</c>.</summary>
public class ClienteUsuario : ClienteApiBase
{
    public ClienteUsuario(HttpClient http) : base(http) { }

    public Task<List<UsuarioVista>> ListarAsync() => GetAsync<List<UsuarioVista>>("api/usuario");

    public Task<UsuarioVista> ObtenerAsync(int id) => GetAsync<UsuarioVista>($"api/usuario/{Escapar(id)}");

    public Task<List<RolDeUsuarioVista>> RolesAsync(int id) => GetAsync<List<RolDeUsuarioVista>>($"api/usuario/{Escapar(id)}/roles");

    public Task CrearAsync(UsuarioFormulario formulario) => PostAsync("api/usuario", formulario);

    public Task ReemplazarAsync(int id, UsuarioFormulario formulario) => PutAsync($"api/usuario/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/usuario/{Escapar(id)}");
}

/// <summary>Cliente HTTP de <c>/api/rol</c>.</summary>
public class ClienteRol : ClienteApiBase
{
    public ClienteRol(HttpClient http) : base(http) { }

    public Task<List<RolVista>> ListarAsync() => GetAsync<List<RolVista>>("api/rol");

    public Task<RolVista> ObtenerAsync(int id) => GetAsync<RolVista>($"api/rol/{Escapar(id)}");

    public Task CrearAsync(RolFormulario formulario) => PostAsync("api/rol", formulario);

    public Task ReemplazarAsync(int id, RolFormulario formulario) => PutAsync($"api/rol/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/rol/{Escapar(id)}");
}

/// <summary>Cliente HTTP de <c>/api/ruta-rol</c>: los permisos de cada rol.</summary>
public class ClientePermiso : ClienteApiBase
{
    public ClientePermiso(HttpClient http) : base(http) { }

    public Task<List<PermisoDeRolVista>> DeRolAsync(int rolId) => GetAsync<List<PermisoDeRolVista>>($"api/ruta-rol/{Escapar(rolId)}");

    public Task DarAsync(int rutaId, int rolId) => PostAsync("api/ruta-rol", new { rutaId, rolId });

    public Task QuitarAsync(int rutaId, int rolId) => DeleteAsync($"api/ruta-rol/{Escapar(rutaId)}/{Escapar(rolId)}");
}
