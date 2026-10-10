using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.AreasConocimiento;

/// <summary>Listado de solo lectura de áreas de conocimiento, con búsqueda por texto (?buscar=).</summary>
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
}
