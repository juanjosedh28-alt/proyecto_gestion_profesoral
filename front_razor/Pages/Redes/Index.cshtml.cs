using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Redes;

/// <summary>Listado de redes con botón Eliminar (borrado lógico en la API).</summary>
public class IndexModel : PageModel
{
    private readonly ClienteRed _cliente;

    public IndexModel(ClienteRed cliente) => _cliente = cliente;

    public List<RedVista> Items { get; private set; } = new();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        try { Items = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { MensajeError = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int idr)
    {
        try
        {
            await _cliente.EliminarAsync(idr);
            TempData["Exito"] = "Registro eliminado correctamente.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
