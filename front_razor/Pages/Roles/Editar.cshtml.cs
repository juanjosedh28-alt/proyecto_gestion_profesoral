using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Roles;

/// <summary>(v3) Editar nombre y descripción de un rol.</summary>
public class EditarModel : PageModel
{
    private readonly ClienteRol _cliente;

    public EditarModel(ClienteRol cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public RolFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var rol = await _cliente.ObtenerAsync(Id);
            Formulario = new RolFormulario { Nombre = rol.Nombre, Descripcion = rol.Descripcion };
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
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.ReemplazarAsync(Id, Formulario);
            TempData["Exito"] = "Rol actualizado.";
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
