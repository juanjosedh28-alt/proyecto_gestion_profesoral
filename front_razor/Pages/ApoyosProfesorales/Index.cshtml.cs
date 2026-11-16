using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.ApoyosProfesorales;

public class IndexModel : PageModel
{
    private readonly ClienteApoyoProfesoral _cliente;
    public IndexModel(ClienteApoyoProfesoral cliente) => _cliente = cliente;

    public List<ApoyoProfesoralVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int estudios)
    {
        try { await _cliente.EliminarAsync(estudios); TempData["Mensaje"] = "Apoyo eliminado."; }
        catch (ErrorApiException ex) { TempData["Mensaje"] = ex.Message; }
        return RedirectToPage();
    }
}
