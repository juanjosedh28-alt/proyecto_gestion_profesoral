using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.TerminosClave;

/// <summary>Listado de términos clave con botón Eliminar (borrado lógico en la API).</summary>
public class IndexModel : PageModel
{
    private readonly ClienteTerminoClave _cliente;

    public IndexModel(ClienteTerminoClave cliente) => _cliente = cliente;

    public List<TerminoClaveVista> Items { get; private set; } = new();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        try { Items = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { MensajeError = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(string termino)
    {
        try
        {
            await _cliente.EliminarAsync(termino);
            TempData["Exito"] = "Registro eliminado correctamente.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
