using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.AreasConocimiento;

/// <summary>Detalle de un área de conocimiento (/AreasConocimiento/Detalle?id=1A01).</summary>
public class DetalleModel : PageModel
{
    private readonly ClienteAreaConocimiento _cliente;

    public DetalleModel(ClienteAreaConocimiento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    public AreaConocimientoVista Area { get; private set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try { Area = await _cliente.ObtenerAsync(Id); return Page(); }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage("Index"); }
    }
}
