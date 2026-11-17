using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EstudiosRealizados;

/// <summary>
/// Formulario para crear un registro de <c>estudios_realizados</c>. La clave foránea (docente)
/// se elige de un desplegable cargado desde la API: muestra el nombre y manda la clave
/// (nunca se digita un código). Se manda como un envío de UN renglón al mismo endpoint que usa el maestro (POST /api/docente/{cedula}/estudios-realizados).
/// Si la API responde 409 o 422, el mensaje queda en el formulario (ErroresFormulario.Aplicar).
/// </summary>
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
