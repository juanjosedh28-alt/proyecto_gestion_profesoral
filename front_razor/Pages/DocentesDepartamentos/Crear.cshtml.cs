using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.DocentesDepartamentos;

public class CrearModel : PageModel
{
    private readonly ClienteDocenteDepartamento _cliente;
    private readonly ClienteDocente _docentes;
    private readonly ClientePrograma _programas;

    public CrearModel(ClienteDocenteDepartamento cliente, ClienteDocente docentes, ClientePrograma programas)
    {
        _cliente = cliente; _docentes = docentes; _programas = programas;
    }

    [BindProperty] public DocenteDepartamentoFormulario Formulario { get; set; } = new();
    public List<DocenteVista> Docentes { get; private set; } = new();
    public List<ProgramaVista> Programas { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Docentes = await _docentes.ListarAsync();
        Programas = await _programas.ListarAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Formulario.Docente <= 0) ModelState.AddModelError("Formulario.Docente", "Seleccione un docente.");
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        try { await _cliente.CrearAsync(Formulario); TempData["Mensaje"] = "Asignación creada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); await OnGetAsync(); return Page(); }
    }
}
