using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.RedesDocentes;

/// <summary>
/// Formulario para crear un registro de <c>red_docente</c>. La clave foránea (red y docente (la pareja es la clave))
/// se elige de un desplegable cargado desde la API: muestra el nombre y manda la clave
/// (nunca se digita un código). Se manda con POST /api/red-docente.
/// Si la API responde 409 o 422, el mensaje queda en el formulario (ErroresFormulario.Aplicar).
/// </summary>
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
