using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EvaluacionesDocente;

/// <summary>
/// Listado de todas las evaluaciones docentes (GET /api/evaluacion-docente).
/// Faltaba este archivo: Index.cshtml decía @model ...IndexModel y la clase
/// no existía, así que el front no compilaba.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ClienteEvaluacion _cliente;
    public IndexModel(ClienteEvaluacion cliente) => _cliente = cliente;

    public List<EvaluacionVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        try { await _cliente.EliminarAsync(id); TempData["Mensaje"] = "Evaluación eliminada."; }
        catch (ErrorApiException ex) { TempData["Mensaje"] = ex.Message; }
        return RedirectToPage();
    }
}
