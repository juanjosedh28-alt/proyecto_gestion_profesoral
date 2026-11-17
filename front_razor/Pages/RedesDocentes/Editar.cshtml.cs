using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.RedesDocentes;

public class EditarModel : PageModel
{
    private readonly ClienteRedDocente _cliente;
    public EditarModel(ClienteRedDocente cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Red { get; set; }
    [BindProperty(SupportsGet = true)] public int Docente { get; set; }
    [BindProperty] public RedDocenteFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Si la pareja no existe (404) se vuelve al listado con el mensaje, en vez de un error 500.
        try { Formulario = RedDocenteFormulario.Desde(await _cliente.ObtenerAsync(Red, Docente)); return Page(); }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage("Index"); }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Red, Docente, Formulario); TempData["Mensaje"] = "Vinculación actualizada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
