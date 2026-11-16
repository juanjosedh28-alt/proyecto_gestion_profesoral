using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.RedesDocentes;

public class CrearModel : PageModel
{
    private readonly ClienteRedDocente _cliente;
    private readonly ClienteRed _redes;
    private readonly ClienteDocente _docentes;

    public CrearModel(ClienteRedDocente cliente, ClienteRed redes, ClienteDocente docentes)
    {
        _cliente = cliente; _redes = redes; _docentes = docentes;
    }

    [BindProperty] public RedDocenteFormulario Formulario { get; set; } = new();
    public List<RedVista> Redes { get; private set; } = new();
    public List<DocenteVista> Docentes { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Redes = await _redes.ListarAsync();
        Docentes = await _docentes.ListarAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Formulario.Docente <= 0) ModelState.AddModelError("Formulario.Docente", "Seleccione un docente.");
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        try { await _cliente.CrearAsync(Formulario); TempData["Mensaje"] = "Vinculación creada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); await OnGetAsync(); return Page(); }
    }
}
