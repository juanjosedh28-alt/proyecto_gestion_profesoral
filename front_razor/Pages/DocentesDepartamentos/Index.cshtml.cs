using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.DocentesDepartamentos;

public class IndexModel : PageModel
{
    private readonly ClienteDocenteDepartamento _cliente;
    public IndexModel(ClienteDocenteDepartamento cliente) => _cliente = cliente;

    public List<DocenteDepartamentoVista> Lista { get; private set; } = new();
    public string? Error { get; private set; }

    public async Task OnGetAsync()
    {
        try { Lista = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { Error = ex.Message; }
    }

    public async Task<IActionResult> OnPostEliminarAsync(int docente, int departamento)
    {
        try { await _cliente.EliminarAsync(docente, departamento); TempData["Mensaje"] = "Asignación eliminada."; }
        catch (ErrorApiException ex) { TempData["Mensaje"] = ex.Message; }
        return RedirectToPage();
    }
}
