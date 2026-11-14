using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Roles;

/// <summary>(v3) Crear un rol. Nace sin permisos: se le dan en «Permisos por rol».</summary>
public class CrearModel : PageModel
{
    private readonly ClienteRol _cliente;

    public CrearModel(ClienteRol cliente) => _cliente = cliente;

    [BindProperty]
    public RolFormulario Formulario { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.CrearAsync(Formulario);
            TempData["Exito"] = "Rol creado. Ahora asígnele sus permisos.";
            return RedirectToPage("Index");
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }
}
