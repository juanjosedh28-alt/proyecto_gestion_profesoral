using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Usuarios;

/// <summary>
/// (v3) Crear un usuario con sus roles. La contraseña viaja a la API, que la
/// convierte en hash bcrypt antes de mandarla a la base. Usuario y roles se
/// guardan en un solo envío.
/// </summary>
public class CrearModel : PageModel
{
    private readonly ClienteUsuario _cliente;
    private readonly ClienteRol _roles;

    public CrearModel(ClienteUsuario cliente, ClienteRol roles)
    {
        _cliente = cliente;
        _roles = roles;
    }

    [BindProperty]
    public UsuarioFormulario Formulario { get; set; } = new();

    public List<RolVista> Roles { get; private set; } = new();

    public async Task OnGetAsync() => Roles = await _roles.ListarAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Formulario.Contrasena))
            ModelState.AddModelError(string.Empty, "La contraseña es obligatoria al crear un usuario.");
        if (Formulario.RolIds.Count == 0)
            ModelState.AddModelError(string.Empty, "Elija al menos un rol.");

        if (!ModelState.IsValid)
        {
            Roles = await _roles.ListarAsync();
            return Page();
        }
        try
        {
            await _cliente.CrearAsync(Formulario);
            TempData["Exito"] = "Usuario creado.";
            return RedirectToPage("Index");
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            Formulario.Contrasena = null;   // no se devuelve la contraseña al navegador
            Roles = await _roles.ListarAsync();
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }
}
