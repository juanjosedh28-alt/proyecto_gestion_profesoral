using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.ApoyosProfesorales;

public class EditarModel : PageModel
{
    private readonly ClienteApoyoProfesoral _cliente;
    public EditarModel(ClienteApoyoProfesoral cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Estudios { get; set; }
    [BindProperty] public ApoyoProfesoralFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        var vista = await _cliente.ObtenerAsync(Estudios);
        if (vista is null) return RedirectToPage("Index");
        Formulario = ApoyoProfesoralFormulario.Desde(vista);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Estudios, Formulario); TempData["Mensaje"] = "Apoyo actualizado."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
