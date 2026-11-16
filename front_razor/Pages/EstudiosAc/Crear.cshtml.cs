using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EstudiosAc;

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
