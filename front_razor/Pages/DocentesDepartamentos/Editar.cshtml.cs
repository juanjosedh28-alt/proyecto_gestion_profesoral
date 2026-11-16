using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.DocentesDepartamentos;

public class EditarModel : PageModel
{
    private readonly ClienteDocenteDepartamento _cliente;
    public EditarModel(ClienteDocenteDepartamento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Docente { get; set; }
    [BindProperty(SupportsGet = true)] public int Departamento { get; set; }
    [BindProperty] public DocenteDepartamentoFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        Formulario = DocenteDepartamentoFormulario.Desde(await _cliente.ObtenerAsync(Docente, Departamento));
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Docente, Departamento, Formulario); TempData["Mensaje"] = "Asignación actualizada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
