using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Sesion;

/// <summary>
/// (v3) Inicio de sesión. Es una de las dos páginas que se abren sin sesión.
///
/// Al pulsar Entrar:
///   1. Manda correo y contraseña a POST /api/login.
///   2. Si la API responde 401, muestra su mensaje (el mismo si falló el correo
///      o la contraseña) y BORRA la contraseña del formulario.
///   3. Si responde 200, guarda en la sesión del servidor el token, el correo,
///      el nombre y los roles; pide las rutas permitidas (para el menú) y
///      lleva a la página que se había pedido, o al inicio.
/// </summary>
public class IniciarModel : PageModel
{
    private readonly ClienteSesion _cliente;

    public IniciarModel(ClienteSesion cliente) => _cliente = cliente;

    [BindProperty]
    public InicioSesionFormulario Formulario { get; set; } = new();

    /// <summary>A dónde volver después de entrar (la página que se había pedido).</summary>
    [BindProperty(SupportsGet = true)]
    public string? Volver { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Motivo { get; set; }

    public string? Aviso => Motivo == "vencida" ? "Su sesión venció o ya no es válida. Inicie sesión de nuevo." : null;

    public IActionResult OnGet()
    {
        // Si ya tiene sesión, no tiene sentido volver a iniciarla.
        if (SesionUsuario.Iniciada(HttpContext.Session)) return Redirect("/");
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            var sesion = await _cliente.IniciarAsync(Formulario.Email.Trim(), Formulario.Contrasena);
            SesionUsuario.Guardar(HttpContext.Session, sesion.Token, sesion.Email, sesion.NombreCompleto, sesion.Roles);

            // Primera petición CON token: las rutas que puede abrir, para el menú.
            var permisos = await _cliente.MisPermisosAsync();
            SesionUsuario.GuardarRutas(HttpContext.Session, permisos.Rutas);

            // Solo se vuelve a una dirección de ESTE sitio (empieza por "/"), nunca a otro dominio.
            return Redirect(!string.IsNullOrEmpty(Volver) && Volver.StartsWith('/') && !Volver.StartsWith("//") ? Volver : "/");
        }
        catch (ErrorApiException ex)
        {
            Formulario.Contrasena = string.Empty;
            ModelState.Clear();
            ModelState.AddModelError(string.Empty, ex.Codigo == 401 ? ex.Message : $"No se pudo iniciar sesión: {ex.Message}");
            return Page();
        }
    }
}
