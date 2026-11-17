using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EstudiosAc;

/// <summary>
/// Formulario para crear un registro de <c>estudio_ac</c>. La clave foránea (estudio y área de conocimiento (puente puro: no se edita, se asigna o se retira))
/// se elige de un desplegable cargado desde la API: muestra el nombre y manda la clave
/// (nunca se digita un código). Se manda con POST /api/estudio-ac.
/// Si la API responde 409 o 422, el mensaje queda en el formulario (ErroresFormulario.Aplicar).
/// </summary>
public class CrearModel : PageModel
{
    private readonly ClienteEstudioAc _cliente;
    private readonly ClienteEstudio _estudios;
    private readonly ClienteAreaConocimiento _areas;

    public CrearModel(ClienteEstudioAc cliente, ClienteEstudio estudios, ClienteAreaConocimiento areas)
    {
        _cliente = cliente; _estudios = estudios; _areas = areas;
    }

    [BindProperty] public EstudioAcFormulario Formulario { get; set; } = new();
    public List<EstudioVista> Estudios { get; private set; } = new();
    public List<AreaConocimientoVista> Areas { get; private set; } = new();

    public async Task OnGetAsync()
    {
        Estudios = await _estudios.ListarAsync();
        Areas = await _areas.ListarAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Formulario.Estudio <= 0) ModelState.AddModelError("Formulario.Estudio", "Seleccione un estudio.");
        if (!ModelState.IsValid) { await OnGetAsync(); return Page(); }
        try { await _cliente.CrearAsync(Formulario); TempData["Mensaje"] = "Área asociada al estudio."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); await OnGetAsync(); return Page(); }
    }
}
