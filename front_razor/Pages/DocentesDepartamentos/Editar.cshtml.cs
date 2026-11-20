using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.DocentesDepartamentos;

/// <summary>
/// Formulario para editar un registro de <c>docente_departamento</c>. La clave y el padre no se cambian:
/// viajan en campos ocultos y en pantalla se muestra el nombre, no el código.
/// Si el registro no existe se vuelve al listado con el aviso, en vez de un error 500.
/// </summary>
public class EditarModel : PageModel
{
    private readonly ClienteDocenteDepartamento _cliente;
    public EditarModel(ClienteDocenteDepartamento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)] public int Docente { get; set; }
    [BindProperty(SupportsGet = true)] public int Departamento { get; set; }
    // Nombres de la pareja, para el título (criterio 5: nombres, no códigos).
    [BindProperty] public string? DocenteNombre { get; set; }
    [BindProperty] public string? ProgramaNombre { get; set; }
    [BindProperty] public DocenteDepartamentoFormulario Formulario { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
    {
        // Si la pareja no existe (404) se vuelve al listado con el mensaje, en vez de un error 500.
        try
        {
            var vista = await _cliente.ObtenerAsync(Docente, Departamento);
            Formulario = DocenteDepartamentoFormulario.Desde(vista);
            DocenteNombre = vista.DocenteNombre;
            ProgramaNombre = vista.DepartamentoNombre;
            return Page();
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; return RedirectToPage("Index"); }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        try { await _cliente.ReemplazarAsync(Docente, Departamento, Formulario); TempData["Mensaje"] = "Asignación actualizada."; return RedirectToPage("Index"); }
        catch (ErrorApiException ex) { ErroresFormulario.Aplicar(ModelState, ex, "Formulario"); return Page(); }
    }
}
