using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Experiencias;

public class EditarModel : PageModel
{
    private readonly ClienteExperiencia _cliente;
    public EditarModel(ClienteExperiencia cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public ExperienciaFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Si el registro no existe (404) se vuelve al listado con el mensaje, en vez de un error 500.
        try { Formulario = ExperienciaFormulario.Desde(await _cliente.ObtenerAsync(Id)); return Page(); }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage("Index"); }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Id, Formulario); TempData["Mensaje"] = "Experiencia actualizada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
