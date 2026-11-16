using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Reconocimientos;

public class EditarModel : PageModel
{
    private readonly ClienteReconocimiento _cliente;
    public EditarModel(ClienteReconocimiento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public ReconocimientoFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync() { Formulario = ReconocimientoFormulario.Desde(await _cliente.ObtenerAsync(Id)); return Page(); }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Id, Formulario); TempData["Mensaje"] = "Reconocimiento actualizado."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
