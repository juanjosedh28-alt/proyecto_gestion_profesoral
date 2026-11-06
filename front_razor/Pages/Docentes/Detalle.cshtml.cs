using front_razor.Clientes;
using front_razor.Infraestructura;
using front_razor.Modelos;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace front_razor.Pages.Docentes;

/// <summary>
/// MAESTRO-DETALLE del docente (criterio 9 de la v2).
///
/// QUÉ HACE: muestra los datos del docente y, EN LA MISMA PANTALLA, su detalle:
/// estudios, evaluaciones, experiencias, reconocimientos, programas, redes e
/// intereses. Cada sección lista, agrega y retira sin volver a elegir el docente:
/// la cédula viene en la ruta (/Docentes/Detalle/1017245).
///
/// POR QUÉ "VARIOS RENGLONES, UN SOLO ENVÍO": en estudios, evaluaciones,
/// experiencias y reconocimientos se pueden escribir varios renglones y se mandan
/// en UNA petición (CrearVariosAsync). Si uno falla, la base no guarda ninguno:
/// "medio registro" no existe.
///
/// POR QUÉ LOS FORMULARIOS SON PARÁMETROS DEL HANDLER y no [BindProperty]: la
/// página tiene siete formularios. Con [BindProperty] se validarían TODOS en cada
/// envío y los vacíos darían error. Como parámetro, solo se valida el que se envió.
///
/// SI FALLA: 409/422 de la API → el mensaje se muestra dentro de la sección y los
/// renglones escritos se conservan; cualquier otro error → aviso arriba.
/// </summary>
public class DetalleModel : PageModel
{
    private readonly ClienteDocente _docentes;
    private readonly ClienteEstudio _estudios;
    private readonly ClienteEvaluacion _evaluaciones;
    private readonly ClienteExperiencia _experiencias;
    private readonly ClienteReconocimiento _reconocimientos;
    private readonly ClienteDocenteDepartamento _departamentos;
    private readonly ClienteRedDocente _redesDocente;
    private readonly ClienteInteresFuturo _intereses;
    private readonly ClientePrograma _programas;
    private readonly ClienteRed _redes;
    private readonly ClienteTerminoClave _terminos;

    public DetalleModel(ClienteDocente docentes, ClienteEstudio estudios, ClienteEvaluacion evaluaciones,
        ClienteExperiencia experiencias, ClienteReconocimiento reconocimientos,
        ClienteDocenteDepartamento departamentos, ClienteRedDocente redesDocente, ClienteInteresFuturo intereses,
        ClientePrograma programas, ClienteRed redes, ClienteTerminoClave terminos)
    {
        _docentes = docentes;
        _estudios = estudios;
        _evaluaciones = evaluaciones;
        _experiencias = experiencias;
        _reconocimientos = reconocimientos;
        _departamentos = departamentos;
        _redesDocente = redesDocente;
        _intereses = intereses;
        _programas = programas;
        _redes = redes;
        _terminos = terminos;
    }

    [BindProperty(SupportsGet = true)]
    public int Cedula { get; set; }

    // ---- Lo que se muestra ----
    public DocenteVista Docente { get; private set; } = new();
    public List<EstudioVista> Estudios { get; private set; } = new();
    public List<EvaluacionVista> Evaluaciones { get; private set; } = new();
    public List<ExperienciaVista> Experiencias { get; private set; } = new();
    public List<ReconocimientoVista> Reconocimientos { get; private set; } = new();
    public List<DocenteDepartamentoVista> Departamentos { get; private set; } = new();
    public List<RedDocenteVista> RedesDelDocente { get; private set; } = new();
    public List<InteresFuturoVista> Intereses { get; private set; } = new();

    // ---- Opciones de los desplegables (salen de la API, solo activos) ----
    public List<ProgramaVista> OpcionesProgramas { get; private set; } = new();
    public List<RedVista> OpcionesRedes { get; private set; } = new();
    public List<TerminoClaveVista> OpcionesTerminos { get; private set; } = new();

    // ---- Formularios "agregar" (se conservan si hubo un error) ----
    public List<EstudioFormulario> NuevosEstudios { get; private set; } = new() { new() };
    public List<EvaluacionFormulario> NuevasEvaluaciones { get; private set; } = new() { new() };
    public List<ExperienciaFormulario> NuevasExperiencias { get; private set; } = new() { new() };
    public List<ReconocimientoFormulario> NuevosReconocimientos { get; private set; } = new() { new() };
    public DocenteDepartamentoFormulario NuevoPrograma { get; private set; } = new();
    public RedDocenteFormulario NuevaRed { get; private set; } = new();
    public InteresFuturoFormulario NuevoInteres { get; private set; } = new();

    /// <summary>Errores por sección ("estudios", "redes"...), para mostrarlos donde ocurrieron.</summary>
    public Dictionary<string, List<string>> Errores { get; } = new();

    public List<string> ErroresDe(string seccion) => Errores.TryGetValue(seccion, out var e) ? e : new();

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            await CargarAsync();
            return Page();
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Index");
        }
    }

    // =================== Agregar (varios renglones, un envío) ===================

    public Task<IActionResult> OnPostAgregarEstudiosAsync(List<EstudioFormulario> estudios)
    {
        NuevosEstudios = estudios;
        return GuardarAsync("estudios", () => _estudios.CrearVariosAsync(Cedula, estudios),
            $"Se agregaron {estudios.Count} estudio(s).");
    }

    public Task<IActionResult> OnPostAgregarEvaluacionesAsync(List<EvaluacionFormulario> evaluaciones)
    {
        NuevasEvaluaciones = evaluaciones;
        return GuardarAsync("evaluaciones", () => _evaluaciones.CrearVariosAsync(Cedula, evaluaciones),
            $"Se agregaron {evaluaciones.Count} evaluación(es).");
    }

    public Task<IActionResult> OnPostAgregarExperienciasAsync(List<ExperienciaFormulario> experiencias)
    {
        NuevasExperiencias = experiencias;
        return GuardarAsync("experiencias", () => _experiencias.CrearVariosAsync(Cedula, experiencias),
            $"Se agregaron {experiencias.Count} experiencia(s).");
    }

    public Task<IActionResult> OnPostAgregarReconocimientosAsync(List<ReconocimientoFormulario> reconocimientos)
    {
        NuevosReconocimientos = reconocimientos;
        return GuardarAsync("reconocimientos", () => _reconocimientos.CrearVariosAsync(Cedula, reconocimientos),
            $"Se agregaron {reconocimientos.Count} reconocimiento(s).");
    }

    // =================== Asignar (tablas puente) ===================

    public Task<IActionResult> OnPostAsignarProgramaAsync(DocenteDepartamentoFormulario programa)
    {
        programa.Docente = Cedula;   // el docente sale de la ruta, no del formulario
        NuevoPrograma = programa;
        return GuardarAsync("programas", () => _departamentos.CrearAsync(programa), "Programa asignado.");
    }

    public Task<IActionResult> OnPostAsignarRedAsync(RedDocenteFormulario red)
    {
        red.Docente = Cedula;
        NuevaRed = red;
        return GuardarAsync("redes", () => _redesDocente.CrearAsync(red), "Red asignada.");
    }

    public Task<IActionResult> OnPostAsignarInteresAsync(InteresFuturoFormulario interes)
    {
        interes.Docente = Cedula;
        NuevoInteres = interes;
        return GuardarAsync("intereses", () => _intereses.CrearAsync(interes), "Interés agregado.");
    }

    // =================== Retirar (borrado lógico) ===================

    public Task<IActionResult> OnPostRetirarEstudioAsync(int id) =>
        RetirarAsync("estudios", () => _estudios.EliminarAsync(id));

    public Task<IActionResult> OnPostRetirarEvaluacionAsync(int id) =>
        RetirarAsync("evaluaciones", () => _evaluaciones.EliminarAsync(id));

    public Task<IActionResult> OnPostRetirarExperienciaAsync(int id) =>
        RetirarAsync("experiencias", () => _experiencias.EliminarAsync(id));

    public Task<IActionResult> OnPostRetirarReconocimientoAsync(int id) =>
        RetirarAsync("reconocimientos", () => _reconocimientos.EliminarAsync(id));

    public Task<IActionResult> OnPostRetirarProgramaAsync(int departamento) =>
        RetirarAsync("programas", () => _departamentos.EliminarAsync(Cedula, departamento));

    public Task<IActionResult> OnPostRetirarRedAsync(int red) =>
        RetirarAsync("redes", () => _redesDocente.EliminarAsync(red, Cedula));

    public Task<IActionResult> OnPostRetirarInteresAsync(string termino) =>
        RetirarAsync("intereses", () => _intereses.EliminarAsync(Cedula, termino));

    // =================== Piezas comunes ===================

    /// <summary>
    /// Patrón de todos los "agregar": si el formulario no es válido o la API responde
    /// 409/422, se vuelve a pintar la página con el error EN ESA SECCIÓN y con lo que
    /// la persona escribió. Si sale bien, se redirige (así recargar no duplica el envío).
    /// </summary>
    private async Task<IActionResult> GuardarAsync(string seccion, Func<Task> accion, string mensajeExito)
    {
        if (!ModelState.IsValid)
            return await RepintarConErrorAsync(seccion, ErroresFormulario.Leer(ModelState));
        try
        {
            await accion();
            TempData["Exito"] = mensajeExito;
            return RedirectToPage("Detalle", null, new { cedula = Cedula }, seccion);
        }
        catch (ErrorApiException ex) when (ex.EsDeFormulario)
        {
            return await RepintarConErrorAsync(seccion, ex.Mensajes().ToList());
        }
        catch (ErrorApiException ex)
        {
            TempData["Error"] = ex.Message;
            return RedirectToPage("Detalle", null, new { cedula = Cedula }, seccion);
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
        return RedirectToPage("Detalle", null, new { cedula = Cedula }, seccion);
    }

    private async Task<IActionResult> RepintarConErrorAsync(string seccion, List<string> mensajes)
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
            return RedirectToPage("Index");
        }
    }

    /// <summary>Carga el docente, sus siete secciones y las opciones de los desplegables.</summary>
    private async Task CargarAsync()
    {
        Docente = await _docentes.ObtenerAsync(Cedula);   // si no existe, la API responde 404
        Estudios = await _estudios.ListarPorDocenteAsync(Cedula);
        Evaluaciones = await _evaluaciones.ListarPorDocenteAsync(Cedula);
        Experiencias = await _experiencias.ListarPorDocenteAsync(Cedula);
        Reconocimientos = await _reconocimientos.ListarPorDocenteAsync(Cedula);
        Departamentos = await _departamentos.ListarPorDocenteAsync(Cedula);
        RedesDelDocente = await _redesDocente.ListarPorDocenteAsync(Cedula);
        Intereses = await _intereses.ListarPorDocenteAsync(Cedula);

        OpcionesProgramas = await _programas.ListarAsync();
        OpcionesRedes = await _redes.ListarAsync();
        OpcionesTerminos = await _terminos.ListarAsync();
    }
}
