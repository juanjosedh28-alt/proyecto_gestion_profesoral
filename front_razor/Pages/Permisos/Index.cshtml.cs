using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Permisos;

/// <summary>
/// (v3) Permisos por rol: se elige un rol y se ve cada ruta con «Tiene» o
/// «No tiene», con un botón para dar o quitar. Es la pantalla del criterio 7:
/// al quitarle una ruta a un rol, sus usuarios la pierden en la siguiente
/// petición, sin cerrar sesión.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ClienteRol _roles;
    private readonly ClientePermiso _permisos;

    public IndexModel(ClienteRol roles, ClientePermiso permisos)
    {
        _roles = roles;
        _permisos = permisos;
    }

    [BindProperty(SupportsGet = true)]
    public int? RolId { get; set; }

    public List<RolVista> Roles { get; private set; } = new();
    public List<PermisoDeRolVista> Permisos { get; private set; } = new();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            Roles = await _roles.ListarAsync();
            if (RolId is int rol) Permisos = await _permisos.DeRolAsync(rol);
        }
        catch (ErrorApiException ex) { MensajeError = ex.Message; }
    }

    public async Task<IActionResult> OnPostDarAsync(int rutaId, int rolId)
    {
        try
        {
            await _permisos.DarAsync(rutaId, rolId);
            TempData["Exito"] = "Permiso dado.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage(new { rolId });
    }

    public async Task<IActionResult> OnPostQuitarAsync(int rutaId, int rolId)
    {
        try
        {
            await _permisos.QuitarAsync(rutaId, rolId);
            TempData["Exito"] = "Permiso quitado. Los usuarios de este rol lo pierden en su siguiente acción.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage(new { rolId });
    }
}
