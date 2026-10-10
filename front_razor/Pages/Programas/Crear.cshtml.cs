using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Programas;

/// <summary>Formulario de creación. Los errores 409/422 de la API se muestran en el formulario.</summary>
public class CrearModel : PageModel
{
    private readonly ClientePrograma _cliente;

    public CrearModel(ClientePrograma cliente) => _cliente = cliente;

    [BindProperty]
    public ProgramaFormulario Formulario { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.CrearAsync(Formulario);
            TempData["Exito"] = "Registro creado correctamente.";
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
