using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.AreasConocimiento;

/// <summary>(v2) Formulario para crear un área de conocimiento. Los errores de la API (código repetido, datos inválidos) se muestran en el formulario.</summary>
public class CrearModel : PageModel
{
    private readonly ClienteAreaConocimiento _cliente;

    public CrearModel(ClienteAreaConocimiento cliente) => _cliente = cliente;

    [BindProperty]
    public AreaConocimientoFormulario Formulario { get; set; } = new();

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.CrearAsync(Formulario);
            TempData["Exito"] = "Área de conocimiento creada correctamente.";
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
