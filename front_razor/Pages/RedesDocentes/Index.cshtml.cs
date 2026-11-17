using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.RedesDocentes;

public class IndexModel : PageModel
{
    private readonly ClienteRedDocente _cliente;
    public IndexModel(ClienteRedDocente cliente) => _cliente = cliente;

    public List<RedDocenteVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int red, int docente)
    {
        try { await _cliente.EliminarAsync(red, docente); TempData["Mensaje"] = "Vinculación eliminada."; }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
