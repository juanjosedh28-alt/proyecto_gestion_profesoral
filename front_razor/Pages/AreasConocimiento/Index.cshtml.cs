using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.AreasConocimiento;

/// <summary>
/// Listado de áreas de conocimiento, con búsqueda por texto (?buscar=). En la v1 era de
/// solo lectura; desde la v2 tiene Nueva, Editar y Eliminar (borrado lógico en la API, D65).
/// </summary>
public class IndexModel : PageModel
{
    private readonly ClienteAreaConocimiento _cliente;

    public IndexModel(ClienteAreaConocimiento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public string? Buscar { get; set; }

    public List<AreaConocimientoVista> Items { get; private set; } = new();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        try
        {
            var todas = await _cliente.ListarAsync();
            if (string.IsNullOrWhiteSpace(Buscar)) { Items = todas; return; }
            var texto = Buscar.Trim();
            Items = todas.Where(a => a.Id.Contains(texto, StringComparison.OrdinalIgnoreCase)
                                  || a.GranArea.Contains(texto, StringComparison.OrdinalIgnoreCase)
                                  || a.Area.Contains(texto, StringComparison.OrdinalIgnoreCase)
                                  || a.Disciplina.Contains(texto, StringComparison.OrdinalIgnoreCase)).ToList();
        }
        catch (ErrorApiException ex) { MensajeError = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(string id)
    {
        try
        {
            await _cliente.EliminarAsync(id);
            TempData["Exito"] = "Área de conocimiento eliminada correctamente.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
