using System.ComponentModel.DataAnnotations;

namespace ApiGestion.Peticiones;

/// <summary>(v3) Cuerpo de POST /api/sesion.</summary>
public class InicioSesion
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [StringLength(150, ErrorMessage = "El correo no puede superar 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [StringLength(100, ErrorMessage = "La contraseña no puede superar 100 caracteres.")]
    public string Contrasena { get; set; } = string.Empty;
}
