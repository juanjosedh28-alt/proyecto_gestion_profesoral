using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Reconocimientos;

/// <summary>
/// Formulario para editar un registro de <c>reconocimiento</c>. La clave y el padre no se cambian:
/// viajan en campos ocultos y en pantalla se muestra el nombre, no el código.
/// Si el registro no existe se vuelve al listado con el aviso, en vez de un error 500.
/// </summary>
public class EditarModel : PageModel
{
    private readonly ClienteReconocimiento _cliente;
    public EditarModel(ClienteReconocimiento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Id { get; set; }
    // Nombre del docente: la clave foránea se muestra con el nombre, no solo con la cédula (criterio 5).
    [BindProperty] public string? DocenteNombre { get; set; }
    [BindProperty] public ReconocimientoFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Si el registro no existe (404) se vuelve al listado con el mensaje, en vez de un error 500.
        try { var vista = await _cliente.ObtenerAsync(Id); Formulario = ReconocimientoFormulario.Desde(vista); DocenteNombre = vista.DocenteNombre; return Page(); }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage("Index"); }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Id, Formulario); TempData["Mensaje"] = "Reconocimiento actualizado."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
