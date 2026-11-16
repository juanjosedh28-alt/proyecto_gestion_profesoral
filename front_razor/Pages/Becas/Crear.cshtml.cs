using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Becas;

public class CrearModel : PageModel
{
    private readonly ClienteBeca _cliente;
    private readonly ClienteEstudio _estudios;
    public CrearModel(ClienteBeca cliente, ClienteEstudio estudios) { _cliente = cliente; _estudios = estudios; }

    [BindProperty] public BecaFormulario Formulario { get; set; } = new();
    public List<EstudioVista> Estudios { get; private set; } = new();

    public async Task OnGetAsync() => Estudios = await _estudios.ListarAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (Formulario.Estudios <= 0) ModelState.AddModelError("Formulario.Estudios", "Seleccione un estudio.");
        if (!ModelState.IsValid) { Estudios = await _estudios.ListarAsync(); return Page(); }
        try { await _cliente.CrearAsync(Formulario); TempData["Mensaje"] = "Beca registrada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); Estudios = await _estudios.ListarAsync(); return Page(); }
    }
}
