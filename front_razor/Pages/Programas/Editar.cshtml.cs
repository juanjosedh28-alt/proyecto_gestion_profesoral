using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Programas;

/// <summary>Formulario de edición (PUT). 404 → vuelve al listado; 409/422 → errores en el formulario.</summary>
public class EditarModel : PageModel
{
    private readonly ClientePrograma _cliente;

    public EditarModel(ClientePrograma cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    [BindProperty]
    public ProgramaFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var v = await _cliente.ObtenerAsync(Id);
            Formulario = new ProgramaFormulario { Id = v.Id, Nombre = v.Nombre, Tipo = v.Tipo, Nivel = v.Nivel, FechaCreacion = v.FechaCreacion, FechaCierre = v.FechaCierre, NumeroCohortes = v.NumeroCohortes, CantGraduados = v.CantGraduados, Ciudad = v.Ciudad, Facultad = v.Facultad };
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
            TempData["Exito"] = "Registro actualizado correctamente.";
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
