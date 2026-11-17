using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EstudiosAc;

public class IndexModel : PageModel
{
    private readonly ClienteEstudioAc _cliente;
    public IndexModel(ClienteEstudioAc cliente) => _cliente = cliente;

    public List<EstudioAcVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int estudio, string area)
    {
        try { await _cliente.EliminarAsync(estudio, area); TempData["Mensaje"] = "Asociación eliminada."; }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
