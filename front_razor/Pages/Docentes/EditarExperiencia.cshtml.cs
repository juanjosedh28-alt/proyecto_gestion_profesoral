using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Docentes;

/// <summary>
/// Pantalla para editar: experiencia del docente. Se llega desde el detalle del docente y
/// se vuelve a él. El docente viene en la ruta: no se vuelve a elegir.
/// 
/// 404 → vuelve al detalle con aviso; 409/422 → errores en el formulario.
/// </summary>
public class EditarExperienciaModel : PageModel
{
    private readonly ClienteExperiencia _cliente;

    public EditarExperienciaModel(ClienteExperiencia cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public int Cedula { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public ExperienciaFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var vista = await _cliente.ObtenerAsync(Id);
            Formulario = ExperienciaFormulario.Desde(vista);
            
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return VolverAlDocente();
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Las claves salen de la ruta, nunca del formulario.
        Formulario.Docente = Cedula;
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.ReemplazarAsync(Id, Formulario);
            TempData["Exito"] = "Cambios guardados.";
            return VolverAlDocente();
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            foreach (var mensaje in ex.Mensajes()) ModelState.AddModelError(string.Empty, mensaje);
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return VolverAlDocente();
        }
    }

    private IActionResult VolverAlDocente() => RedirectToPage("Detalle", null, new { cedula = Cedula }, "experiencias");
}
