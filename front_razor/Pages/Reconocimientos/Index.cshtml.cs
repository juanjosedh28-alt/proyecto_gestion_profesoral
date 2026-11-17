using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Reconocimientos;

public class IndexModel : PageModel
{
    private readonly ClienteReconocimiento _cliente;
    public IndexModel(ClienteReconocimiento cliente) => _cliente = cliente;

    public List<ReconocimientoVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        try { await _cliente.EliminarAsync(id); TempData["Mensaje"] = "Reconocimiento eliminado."; }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
