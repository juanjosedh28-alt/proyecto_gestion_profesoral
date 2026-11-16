using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.InteresesFuturos;

public class CrearModel : PageModel
{
    private readonly ClienteInteresFuturo _cliente;
    private readonly ClienteDocente _docentes;
    private readonly ClienteTerminoClave _terminos;

    public CrearModel(ClienteInteresFuturo cliente, ClienteDocente docentes, ClienteTerminoClave terminos)
    {
        _cliente = cliente; _docentes = docentes; _terminos = terminos;
    }

    [BindProperty] public InteresFuturoFormulario Formulario { get; set; } = new();
    public List<DocenteVista> Docentes { get; private set; } = new();
    public List<TerminoClaveVista> Terminos { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Docentes = await _docentes.ListarAsync();
        Terminos = await _terminos.ListarAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Formulario.Docente <= 0) ModelState.AddModelError("Formulario.Docente", "Seleccione un docente.");
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        try { await _cliente.CrearAsync(Formulario); TempData["Mensaje"] = "Interés asociado."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); await OnGetAsync(); return Page(); }
    }
}
