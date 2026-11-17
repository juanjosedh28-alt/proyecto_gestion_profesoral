using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EvaluacionesDocente;

public class EditarModel : PageModel
{
    private readonly ClienteEvaluacion _cliente;
    public EditarModel(ClienteEvaluacion cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public EvaluacionFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Si el registro no existe (404) se vuelve al listado con el mensaje, en vez de un error 500.
        try { Formulario = EvaluacionFormulario.Desde(await _cliente.ObtenerAsync(Id)); return Page(); }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage("Index"); }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Id, Formulario); TempData["Mensaje"] = "Evaluación actualizada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
