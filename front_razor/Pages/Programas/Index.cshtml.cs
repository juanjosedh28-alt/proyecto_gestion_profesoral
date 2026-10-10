using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Programas;

/// <summary>Listado de programas con botón Eliminar (borrado lógico en la API).</summary>
public class IndexModel : PageModel
{
    private readonly ClientePrograma _cliente;

    public IndexModel(ClientePrograma cliente) => _cliente = cliente;

    public List<ProgramaVista> Items { get; private set; } = new();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        try { Items = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { MensajeError = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        try
        {
            await _cliente.EliminarAsync(id);
            TempData["Exito"] = "Registro eliminado correctamente.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
