using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EstudiosRealizados;

public class CrearModel : PageModel
{
    private readonly ClienteEstudio _cliente;
    private readonly ClienteDocente _docentes;
    public CrearModel(ClienteEstudio cliente, ClienteDocente docentes) { _cliente = cliente; _docentes = docentes; }

    [BindProperty] public EstudioFormulario Formulario { get; set; } = new();
    public List<DocenteVista> Docentes { get; private set; } = new();

    public async Task OnGetAsync() => Docentes = await _docentes.ListarAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (Formulario.Docente <= 0) ModelState.AddModelError("Formulario.Docente", "Seleccione un docente.");
        if (!ModelState.IsValid) { Docentes = await _docentes.ListarAsync(); return Page(); }
        try
        {
            await _cliente.CrearVariosAsync(Formulario.Docente, new List<EstudioFormulario> { Formulario });
            TempData["Mensaje"] = "Estudio registrado.";
            return RedirectToPage("Index");
        }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); Docentes = await _docentes.ListarAsync(); return Page(); }
    }
}
