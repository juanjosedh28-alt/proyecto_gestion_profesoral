using front_razor.Infraestructura;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Sesion;

/// <summary>
/// (v3) Cerrar sesión: borra la sesión del servidor (y con ella el token) y
/// lleva al inicio de sesión. Es un POST (botón «Salir» en la barra) para
/// que un enlace en otra página no pueda cerrarle la sesión a nadie.
///
/// El JWT no se puede «apagar» en la API: sigue siendo válido hasta que vence.
/// Por eso lo importante es que el front lo olvide, y que dure poco (60 min).
/// </summary>
public class SalirModel : PageModel
{
    public IActionResult OnGet() => Redirect("/");

    public IActionResult OnPost()
    {
        SesionUsuario.Cerrar(HttpContext.Session);
        TempData["Exito"] = "Sesión cerrada.";
        return RedirectToPage("/Sesion/Iniciar");
    }
}
