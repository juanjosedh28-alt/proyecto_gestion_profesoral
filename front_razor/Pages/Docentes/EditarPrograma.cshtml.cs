using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Docentes;

/// <summary>
/// Pantalla para editar: vinculación a programa del docente. Se llega desde el detalle del docente y
/// se vuelve a él. El docente viene en la ruta: no se vuelve a elegir.
/// La pareja (docente y programa) es la clave: no se cambia, solo se editan sus datos. Para cambiarla, se retira y se asigna otra.
/// 404 → vuelve al detalle con aviso; 409/422 → errores en el formulario.
/// </summary>
public class EditarProgramaModel : PageModel
{
    private readonly ClienteDocenteDepartamento _cliente;

    public EditarProgramaModel(ClienteDocenteDepartamento cliente) => _cliente = cliente;

    [BindProperty(SupportsGet = true)]
    public int Cedula { get; set; }

    [BindProperty(SupportsGet = true)]
    public int Departamento { get; set; }

    [BindProperty]
    public DocenteDepartamentoFormulario Formulario { get; set; } = new();

    /// <summary>Lo que no se edita (la clave), solo se muestra.</summary>
    public string Clave { get; private set; } = string.Empty;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var vista = await _cliente.ObtenerAsync(Cedula, Departamento);
            Formulario = DocenteDepartamentoFormulario.Desde(vista);
            Clave = vista.DepartamentoNombre.ToString();
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
        Formulario.Departamento = Departamento;
        if (!ModelState.IsValid) return Page();
        try
        {
            await _cliente.ReemplazarAsync(Cedula, Departamento, Formulario);
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

    private IActionResult VolverAlDocente() => RedirectToPage("Detalle", null, new { cedula = Cedula }, "programas");
}
