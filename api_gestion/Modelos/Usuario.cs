namespace ApiGestion.Modelos;

/// <summary>
/// (v3) Un usuario tal como lo devuelve la API. NO tiene la contraseña ni su
/// hash: la vista vw_usuario ni siquiera trae esa columna, así que ningún
/// listado puede devolverla por descuido.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
    public string FechaActualizacion { get; set; } = string.Empty;
    /// <summary>Sus roles activos, separados por coma (para mostrar).</summary>
    public string Roles { get; set; } = string.Empty;
}

/// <summary>(v3) Un rol asignado a un usuario (fila de rol_usuario con el nombre del rol).</summary>
public class RolUsuario
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
}

/// <summary>(v3) Un rol, con cuántos usuarios activos lo tienen.</summary>
public class Rol
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int TotalUsuarios { get; set; }
}

/// <summary>(v3) Una ruta protegible (interfaz.docentes, datos.modificar…).</summary>
public class Ruta
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
}

/// <summary>(v3) Una ruta y si un rol la tiene o no: una casilla de la pantalla de permisos.</summary>
public class PermisoDeRol
{
    public int RutaId { get; set; }
    public string RutaNombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Asignada { get; set; }
}
