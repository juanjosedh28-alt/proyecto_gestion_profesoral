using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Experiencias;

/// <summary>
/// Pantalla propia de la tabla <c>experiecia</c>: lista las experiencias laborales de todos los docentes
/// (GET /api/experiecia). Existe para que cada tabla del módulo se vea desde el menú y la página
/// de inicio; el maestro-detalle sigue en Docentes → Detalle.
/// Retirar es borrado LÓGICO: la API marca activo = 0, el registro sale del listado y
/// sigue en la base. Si la API lo impide (409), el mensaje sale en rojo.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ClienteExperiencia _cliente;
    public IndexModel(ClienteExperiencia cliente) => _cliente = cliente;

    public List<ExperienciaVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int id)
    {
        try { await _cliente.EliminarAsync(id); TempData["Mensaje"] = "Experiencia eliminada."; }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
