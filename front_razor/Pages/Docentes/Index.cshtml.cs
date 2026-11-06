using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Docentes;

/// <summary>
/// Listado de docentes ACTIVOS. Muestra el total de estudios, que mantiene el
/// disparador de la base (el front nunca lo envía). Retirar es un borrado lógico;
/// si el docente todavía tiene detalle activo, la API responde 409 y aquí se
/// muestra su mensaje en castellano.
/// </summary>
public class IndexModel : PageModel
{
    private readonly ClienteDocente _cliente;

    public IndexModel(ClienteDocente cliente) => _cliente = cliente;

    public List<DocenteVista> Items { get; private set; } = new();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        try { Items = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { MensajeError = ex.Message; }
    }

    public async Task<IActionResult> OnPostRetirarAsync(int cedula)
    {
        try
        {
            await _cliente.EliminarAsync(cedula);
            TempData["Exito"] = "Docente retirado correctamente.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
