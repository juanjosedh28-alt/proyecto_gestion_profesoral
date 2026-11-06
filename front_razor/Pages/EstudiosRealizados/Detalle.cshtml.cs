using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.EstudiosRealizados;

/// <summary>
/// MAESTRO-DETALLE del estudio: el estudio es maestro de apoyo_profesoral, beca
/// y estudio_ac. Se llega desde el detalle del docente.
///
/// Apoyo y beca son 1:1 (un estudio tiene a lo sumo uno de cada uno): por eso
/// hay UN formulario que crea si no existe y actualiza si ya existe.
/// Las áreas de conocimiento son un puente puro: se asignan (desplegable) y se
/// retiran; no se editan.
/// </summary>
public class DetalleModel : PageModel
{
    private readonly ClienteEstudio _estudios;
    private readonly ClienteApoyoProfesoral _apoyos;
    private readonly ClienteBeca _becas;
    private readonly ClienteEstudioAc _areasDelEstudio;
    private readonly ClienteAreaConocimiento _areas;

    public DetalleModel(ClienteEstudio estudios, ClienteApoyoProfesoral apoyos, ClienteBeca becas,
        ClienteEstudioAc areasDelEstudio, ClienteAreaConocimiento areas)
    {
        _estudios = estudios;
        _apoyos = apoyos;
        _becas = becas;
        _areasDelEstudio = areasDelEstudio;
        _areas = areas;
    }

    [BindProperty(SupportsGet = true)]
    public int Id { get; set; }

    public EstudioVista Estudio { get; private set; } = new();
    public ApoyoProfesoralVista? Apoyo { get; private set; }
    public BecaVista? Beca { get; private set; }
    public List<EstudioAcVista> Areas { get; private set; } = new();
    public List<AreaConocimientoVista> OpcionesAreas { get; private set; } = new();

    public ApoyoProfesoralFormulario FormApoyo { get; private set; } = new();
    public BecaFormulario FormBeca { get; private set; } = new();

    public Dictionary<string, List<string>> Errores { get; } = new();
    public List<string> ErroresDe(string seccion) => Errores.TryGetValue(seccion, out var e) ? e : new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await CargarAsync();
            if (Apoyo != null) FormApoyo = ApoyoProfesoralFormulario.Desde(Apoyo);
            if (Beca != null) FormBeca = BecaFormulario.Desde(Beca);
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("/Docentes/Index");
        }
    }

    /// <summary>Crea el apoyo si el estudio no tiene; si ya tiene, lo actualiza.</summary>
    public async Task<IActionResult> OnPostGuardarApoyoAsync(ApoyoProfesoralFormulario apoyo, bool existe)
    {
        apoyo.Estudios = Id;   // el estudio sale de la ruta
        FormApoyo = apoyo;
        return await GuardarAsync("apoyo",
            () => existe ? _apoyos.ReemplazarAsync(Id, apoyo) : _apoyos.CrearAsync(apoyo), "Apoyo profesoral guardado.");
    }

    public Task<IActionResult> OnPostRetirarApoyoAsync() =>
        RetirarAsync("apoyo", () => _apoyos.EliminarAsync(Id));

    public async Task<IActionResult> OnPostGuardarBecaAsync(BecaFormulario beca, bool existe)
    {
        beca.Estudios = Id;
        FormBeca = beca;
        return await GuardarAsync("beca",
            () => existe ? _becas.ReemplazarAsync(Id, beca) : _becas.CrearAsync(beca), "Beca guardada.");
    }

    public Task<IActionResult> OnPostRetirarBecaAsync() =>
        RetirarAsync("beca", () => _becas.EliminarAsync(Id));

    public Task<IActionResult> OnPostAsignarAreaAsync(EstudioAcFormulario area)
    {
        area.Estudio = Id;
        return GuardarAsync("areas", () => _areasDelEstudio.CrearAsync(area), "Área asignada.");
    }

    public Task<IActionResult> OnPostRetirarAreaAsync(string area) =>
        RetirarAsync("areas", () => _areasDelEstudio.EliminarAsync(Id, area));

    // ---- Piezas comunes (mismo patrón que el detalle del docente) ----

    private async Task<IActionResult> GuardarAsync(string seccion, Func<Task> accion, string mensajeExito)
    {
        if (!ModelState.IsValid) return await RepintarAsync(seccion, ErroresFormulario.Leer(ModelState));
        try
        {
            await accion();
            TempData["Exito"] = mensajeExito;
            return RedirectToPage("Detalle", null, new { id = Id }, seccion);
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            return await RepintarAsync(seccion, ex.Mensajes().ToList());
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Detalle", null, new { id = Id }, seccion);
        }
    }

    private async Task<IActionResult> RetirarAsync(string seccion, Func<Task> accion)
    {
        try
        {
            await accion();
            TempData["Exito"] = "Registro retirado.";
        }
        catch (ErrorApiException ex) { TempData["Error"] = ex.Message; }
        return RedirectToPage("Detalle", null, new { id = Id }, seccion);
    }

    private async Task<IActionResult> RepintarAsync(string seccion, List<string> mensajes)
    {
        Errores[seccion] = mensajes;
        try
        {
            await CargarAsync();
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("/Docentes/Index");
        }
    }

    private async Task CargarAsync()
    {
        Estudio = await _estudios.ObtenerAsync(Id);       // 404 si no existe o está retirado
        Apoyo = await _apoyos.ObtenerAsync(Id);           // null si todavía no tiene
        Beca = await _becas.ObtenerAsync(Id);
        Areas = await _areasDelEstudio.ListarPorEstudioAsync(Id);
        OpcionesAreas = await _areas.ListarAsync();
    }
}
