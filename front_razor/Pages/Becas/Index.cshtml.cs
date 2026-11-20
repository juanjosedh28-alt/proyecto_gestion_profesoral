using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Becas;

/// <summary>
/// Pantalla propia de la tabla <c>beca</c>: lista las becas de todos los docentes
/// (GET /api/beca). Existe para que cada tabla del módulo se vea desde el menú y la página
/// de inicio; el maestro-detalle sigue en Docentes → Detalle.
/// Retirar es borrado LÓGICO: la API marca activo = 0, el registro sale del listado y
/// sigue en la base. Si la API lo impide (409), el mensaje sale en rojo.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ClienteBeca _cliente;
    public IndexModel(ClienteBeca cliente) => _cliente = cliente;

    public List<BecaVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int estudios)
    {
        try { await _cliente.EliminarAsync(estudios); TempData["Mensaje"] = "Beca eliminada."; }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
