using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Docentes;

/// <summary>
/// Crear docente. El desplegable de líneas de investigación se carga desde la API
/// al abrir la página Y al volver a pintarla tras un error (si no, quedaría vacío).
/// </summary>
public class CrearModel : PageModel
{
    private readonly ClienteDocente _cliente;
    private readonly ClienteLineaInvestigacion _lineas;

    public CrearModel(ClienteDocente cliente, ClienteLineaInvestigacion lineas)
    {
        _cliente = cliente;
        _lineas = lineas;
    }

    [BindProperty]
    public DocenteFormulario Formulario { get; set; } = new();

    public List<LineaInvestigacionVista> Lineas { get; private set; } = new();

    public async Task OnGetAsync() => await CargarLineasAsync();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarLineasAsync();
            return Page();
        }
        try
        {
            await _cliente.CrearAsync(Formulario);
            TempData["Exito"] = "Docente creado correctamente.";
            return RedirectToPage("Detalle", new { cedula = Formulario.Cedula });
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            await CargarLineasAsync();
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }

    private async Task CargarLineasAsync()
    {
        try { Lineas = await _lineas.ListarAsync(); }
        catch (ErrorApiException ex) { ModelState.AddModelError(string.Empty, ex.Message); }
    }
}
