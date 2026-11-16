using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EstudiosRealizados;

public class EditarModel : PageModel
{
    private readonly ClienteEstudio _cliente;
    public EditarModel(ClienteEstudio cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    [BindProperty] public EstudioFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync() { Formulario = EstudioFormulario.Desde(await _cliente.ObtenerAsync(Id)); return Page(); }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Id, Formulario); TempData["Mensaje"] = "Estudio actualizado."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
