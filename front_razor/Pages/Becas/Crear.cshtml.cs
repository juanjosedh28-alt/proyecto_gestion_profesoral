using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Becas;

/// <summary>
/// Formulario para crear un registro de <c>beca</c>. La clave foránea (estudio (una por estudio: su clave es la del estudio))
/// se elige de un desplegable cargado desde la API: muestra el nombre y manda la clave
/// (nunca se digita un código). Se manda con POST /api/beca.
/// Si la API responde 409 o 422, el mensaje queda en el formulario (ErroresFormulario.Aplicar).
/// </summary>
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
