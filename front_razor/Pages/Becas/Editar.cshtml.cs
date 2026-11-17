using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Becas;

public class EditarModel : PageModel
{
    private readonly ClienteBeca _cliente;
    public EditarModel(ClienteBeca cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Estudios { get; set; }
    // Título del estudio: se muestra en vez del número (criterio 5).
    [BindProperty] public string? EstudioTitulo { get; set; }
    [BindProperty] public BecaFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var vista = await _cliente.ObtenerAsync(Estudios);
        if (vista is null) return RedirectToPage("Index");
        Formulario = BecaFormulario.Desde(vista);
        EstudioTitulo = vista.EstudioTitulo;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Estudios, Formulario); TempData["Mensaje"] = "Beca actualizada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
