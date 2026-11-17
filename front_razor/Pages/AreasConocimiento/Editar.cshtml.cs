using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.AreasConocimiento;

/// <summary>
/// (v2) Formulario de edición de un área de conocimiento. El código no se cambia
/// (es la clave). 404 → vuelve al listado con el aviso; 409/422 → errores en el formulario.
/// </summary>
public class EditarModel : PageModel
{
    private readonly ClienteAreaConocimiento _cliente;

    public EditarModel(ClienteAreaConocimiento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public string Id { get; set; } = string.Empty;

    [BindProperty]
    public AreaConocimientoFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var v = await _cliente.ObtenerAsync(Id);
            Formulario = new AreaConocimientoFormulario { Id = v.Id, GranArea = v.GranArea, Area = v.Area, Disciplina = v.Disciplina };
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.ReemplazarAsync(Id, Formulario);
            TempData["Exito"] = "Área de conocimiento actualizada correctamente.";
            return RedirectToPage("Index");
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }
}
