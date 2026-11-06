using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Docentes;

/// <summary>
/// Editar docente (la cédula queda bloqueada: es la clave). La fecha de
/// actualización no se toca aquí: la pone el disparador trg_sello_docente.
/// </summary>
public class EditarModel : PageModel
{
    private readonly ClienteDocente _cliente;
    private readonly ClienteLineaInvestigacion _lineas;

    public EditarModel(ClienteDocente cliente, ClienteLineaInvestigacion lineas)
    {
        _cliente = cliente;
        _lineas = lineas;
    }

    [BindProperty(SupportsGet = true)]
    public int Cedula { get; set; }

    [BindProperty]
    public DocenteFormulario Formulario { get; set; } = new();

    public List<LineaInvestigacionVista> Lineas { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            Formulario = DocenteFormulario.Desde(await _cliente.ObtenerAsync(Cedula));
            await CargarLineasAsync();
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await CargarLineasAsync();
            return Page();
        }
        try
        {
            await _cliente.ReemplazarAsync(Cedula, Formulario);
            TempData["Exito"] = "Docente actualizado correctamente.";
            return RedirectToPage("Detalle", new { cedula = Cedula });
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
