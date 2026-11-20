using System.ComponentModel.DataAnnotations;

namespace front_razor.Modelos;

// ===================================================================
// (v3) Modelos del control de acceso: inicio de sesión, permisos del menú
// y administración de usuarios, roles y permisos.
// ===================================================================

/// <summary>Formulario de inicio de sesión.</summary>
public class InicioSesionFormulario
{
    [Display(Name = "Correo")]
    [Required(ErrorMessage = "Escriba su correo.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Contraseña")]
    [Required(ErrorMessage = "Escriba su contraseña.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;
}

/// <summary>Respuesta de POST /api/sesion.</summary>
public class SesionRespuesta
{
    public string Token { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public DateTime Expira { get; set; }
}

/// <summary>Respuesta de GET /api/permisos/mios.</summary>
public class MisPermisos
{
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public List<string> Rutas { get; set; } = new();
}

public class UsuarioVista
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? NombreCompleto { get; set; }
    public string FechaCreacion { get; set; } = string.Empty;
    public string FechaActualizacion { get; set; } = string.Empty;
    public string Roles { get; set; } = string.Empty;
}

public class RolDeUsuarioVista
{
    public int UsuarioId { get; set; }
    public int RolId { get; set; }
    public string RolNombre { get; set; } = string.Empty;
}

/// <summary>
/// Formulario de usuario. La contraseña es obligatoria al crear (lo revisa la
/// página) y opcional al editar: vacía = no se cambia.
/// </summary>
public class UsuarioFormulario
{
    [Display(Name = "Nombre de usuario")]
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres.")]
    public string Username { get; set; } = string.Empty;

    [Display(Name = "Correo")]
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "Nombre completo")]
    [StringLength(200, ErrorMessage = "El nombre completo no puede superar 200 caracteres.")]
    public string? NombreCompleto { get; set; }

    [Display(Name = "Contraseña")]
    [DataType(DataType.Password)]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "La contraseña debe tener al menos 8 caracteres.")]
    public string? Contrasena { get; set; }

    /// <summary>Los ids de los roles marcados (casillas).</summary>
    public List<int> RolIds { get; set; } = new();
}

public class RolVista
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int TotalUsuarios { get; set; }
}

public class RolFormulario
{
    [Display(Name = "Nombre")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Display(Name = "Descripción")]
    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Descripcion { get; set; }
}

/// <summary>Una ruta y si el rol la tiene (una fila de la pantalla de permisos).</summary>
public class PermisoDeRolVista
{
    public int RutaId { get; set; }
    public string RutaNombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int RolId { get; set; }
    public bool Asignada { get; set; }
}
