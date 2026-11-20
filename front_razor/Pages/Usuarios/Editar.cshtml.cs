using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Usuarios;

/// <summary>
/// (v3) Editar un usuario y sus roles. La contraseña vacía significa «déjela
/// como está». Los roles marcados reemplazan a los que tenía.
/// </summary>
public class EditarModel : PageModel
{
    private readonly ClienteUsuario _cliente;
    private readonly ClienteRol _roles;

    public EditarModel(ClienteUsuario cliente, ClienteRol roles)
    {
        _cliente = cliente;
        _roles = roles;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public UsuarioFormulario Formulario { get; set; } = new();

    public List<RolVista> Roles { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var u = await _cliente.ObtenerAsync(Id);
            var suyos = await _cliente.RolesAsync(Id);
            Formulario = new UsuarioFormulario
            {
                Username = u.Username,
                Email = u.Email,
                NombreCompleto = u.NombreCompleto,
                RolIds = suyos.Select(r => r.RolId).ToList()
            };
            Roles = await _roles.ListarAsync();
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Formulario.RolIds.Count == 0)
            ModelState.AddModelError(string.Empty, "Elija al menos un rol.");
        if (!ModelState.IsValid)
        {
            Roles = await _roles.ListarAsync();
            return Page();
        }
        try
        {
            await _cliente.ReemplazarAsync(Id, Formulario);
            TempData["Exito"] = "Usuario actualizado.";
            return RedirectToPage("Index");
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            Formulario.Contrasena = null;
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
