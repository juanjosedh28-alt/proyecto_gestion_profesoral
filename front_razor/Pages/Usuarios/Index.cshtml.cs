using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Usuarios;

/// <summary>
/// (v3) Listado de usuarios. Esta página NO revisa permisos: si quien la abre
/// no tiene «interfaz.usuarios», la API responde 403 al pedir la lista y se
/// muestra «No tiene permiso». Así se comprueba que la protección está en la
/// API y no en el menú (criterio 9).
/// </summary>
public class IndexModel : PageModel
{
    private readonly ClienteUsuario _cliente;

    public IndexModel(ClienteUsuario cliente) => _cliente = cliente;

    public List<UsuarioVista> Items { get; private set; } = new();
    public string? MensajeError { get; private set; }

    public async Task OnGetAsync()
    {
        try { Items = await _cliente.ListarAsync(); }
        catch (ErrorApiException ex) { MensajeError = ex.Message; }
    }

    public async Task<IActionResult> OnPostRetirarAsync(int id)
    {
        try
        {
            await _cliente.EliminarAsync(id);
            TempData["Exito"] = "Usuario retirado. Ya no puede iniciar sesión y su sesión abierta deja de servir.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage();
    }
}
