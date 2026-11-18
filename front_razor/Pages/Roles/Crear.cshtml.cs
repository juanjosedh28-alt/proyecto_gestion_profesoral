using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Roles;

/// <summary>
/// (v3) Crear un rol. Nace sin permisos: se le dan en «Permisos por rol».
///
/// Al abrirla pide a la API los roles que ya existen (GET /api/rol) para
/// mostrarlos y no repetir un nombre. Esa petición es también la que hace que
/// la API decida: quien no tiene «interfaz.roles» y escribe /Roles/Crear a
/// mano recibe el 403, igual que en el listado (criterio 9).
/// </summary>
public class CrearModel : PageModel
{
    private readonly ClienteRol _cliente;

    public CrearModel(ClienteRol cliente) => _cliente = cliente;

    [BindProperty]
    public RolFormulario Formulario { get; set; } = new();

    /// <summary>Los roles que ya existen.</summary>
    public List<RolVista> Existentes { get; private set; } = new();

    public async Task OnGetAsync() => Existentes = await _cliente.ListarAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return await PaginaAsync();
        try
        {
            await _cliente.CrearAsync(Formulario);
            TempData["Exito"] = "Rol creado. Ahora asígnele sus permisos.";
            return RedirectToPage("Index");
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            return await PaginaAsync();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }

    /// <summary>Vuelve a mostrar el formulario con los roles que ya existen.</summary>
    private async Task<IActionResult> PaginaAsync()
    {
        Existentes = await _cliente.ListarAsync();
        return Page();
    }
}
