using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.TerminosClave;

/// <summary>Formulario de edición (PUT). 404 → vuelve al listado; 409/422 → errores en el formulario.</summary>
public class EditarModel : PageModel
{
    private readonly ClienteTerminoClave _cliente;

    public EditarModel(ClienteTerminoClave cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public string Termino { get; set; } = string.Empty;

    [BindProperty]
    public TerminoClaveFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var v = await _cliente.ObtenerAsync(Termino);
            Formulario = new TerminoClaveFormulario { Termino = v.Termino, TerminoIngles = v.TerminoIngles };
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
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.ReemplazarAsync(Termino, Formulario);
            TempData["Exito"] = "Registro actualizado correctamente.";
            return RedirectToPage("Index");
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }
}
