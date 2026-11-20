using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>
/// (v3) Cuerpo de POST y PUT de /api/usuario.
///
/// La CONTRASEÑA llega en claro por la petición (por eso, en producción, solo
/// con HTTPS) y la API la convierte en hash ANTES de mandarla a la base: la
/// base nunca la ve.
/// - Al crear es obligatoria (mínimo 8 caracteres).
/// - Al editar puede llegar VACÍA: eso significa «déjela como está», no «bórrela».
/// </summary>
public class UsuarioCrear
{
    [Required(ErrorMessage = "El nombre de usuario es obligatorio.")]
    [StringLength(100, MinimumLength = 3, ErrorMessage = "El nombre de usuario debe tener entre 3 y 100 caracteres.")]
    [RegularExpression(@"^[a-zA-Z0-9._-]+$", ErrorMessage = "El nombre de usuario solo admite letras sin tilde, números, punto, guion y guion bajo.")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    [EmailAddress(ErrorMessage = "El correo no tiene un formato válido.")]
    public string Email { get; set; } = string.Empty;

    [StringLength(200, ErrorMessage = "El nombre completo no puede superar 200 caracteres.")]
    public string? NombreCompleto { get; set; }

    [StringLength(100, ErrorMessage = "La contraseña no puede superar 100 caracteres.")]
    public string? Contrasena { get; set; }

    [Required(ErrorMessage = "Elija al menos un rol.")]
    [MinLength(1, ErrorMessage = "Elija al menos un rol.")]
    public List<int> RolIds { get; set; } = new();
}

/// <summary>(v3) Cuerpo de POST /api/rol-usuario: asignar un rol a un usuario.</summary>
public class RolUsuarioCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "El usuario es obligatorio.")]
    public int UsuarioId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El rol es obligatorio.")]
    public int RolId { get; set; }
}

/// <summary>(v3) Cuerpo de POST y PUT de /api/rol.</summary>
public class RolCrear
{
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre no puede superar 100 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [StringLength(500, ErrorMessage = "La descripción no puede superar 500 caracteres.")]
    public string? Descripcion { get; set; }
}

/// <summary>(v3) Cuerpo de POST /api/ruta-rol: darle un permiso (ruta) a un rol.</summary>
public class RutaRolCrear
{
    [Range(1, int.MaxValue, ErrorMessage = "La ruta es obligatoria.")]
    public int RutaId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El rol es obligatorio.")]
    public int RolId { get; set; }
}
