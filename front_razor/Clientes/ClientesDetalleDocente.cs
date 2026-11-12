using front_razor.Modelos;

namespace front_razor.Clientes;

// ===================================================================
// Clientes HTTP del DETALLE del docente. Cada uno tiene:
//   - ListarPorDocenteAsync: GET /api/docente/{cedula}/<detalle>  (el detalle se ve desde el maestro)
//   - CrearVariosAsync:      POST /api/docente/{cedula}/<detalle> con una LISTA
//                            -> varios renglones en UNA sola petición
//   - Obtener / Reemplazar / Eliminar por su id.
// ===================================================================

/// <summary>Cliente HTTP de <c>/api/estudios-realizados</c>.</summary>
public class ClienteEstudio : ClienteApiBase
{
    public ClienteEstudio(HttpClient http) : base(http) { }

    public Task<List<EstudioVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<EstudioVista>>($"api/docente/{Escapar(cedula)}/estudios-realizados");

    public Task CrearVariosAsync(int cedula, List<EstudioFormulario> renglones) =>
        PostAsync($"api/docente/{Escapar(cedula)}/estudios-realizados", renglones);

    public Task<EstudioVista> ObtenerAsync(int id) => GetAsync<EstudioVista>($"api/estudios-realizados/{Escapar(id)}");

    public Task ReemplazarAsync(int id, EstudioFormulario formulario) => PutAsync($"api/estudios-realizados/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/estudios-realizados/{Escapar(id)}");
}

/// <summary>Cliente HTTP de <c>/api/evaluacion-docente</c>.</summary>
public class ClienteEvaluacion : ClienteApiBase
{
    public ClienteEvaluacion(HttpClient http) : base(http) { }

    public Task<List<EvaluacionVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<EvaluacionVista>>($"api/docente/{Escapar(cedula)}/evaluacion-docente");

    public Task CrearVariosAsync(int cedula, List<EvaluacionFormulario> renglones) =>
        PostAsync($"api/docente/{Escapar(cedula)}/evaluacion-docente", renglones);

    public Task<EvaluacionVista> ObtenerAsync(int id) => GetAsync<EvaluacionVista>($"api/evaluacion-docente/{Escapar(id)}");

    public Task ReemplazarAsync(int id, EvaluacionFormulario formulario) => PutAsync($"api/evaluacion-docente/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/evaluacion-docente/{Escapar(id)}");
}

/// <summary>Cliente HTTP de <c>/api/experiecia</c> (la ruta conserva la errata del esquema).</summary>
public class ClienteExperiencia : ClienteApiBase
{
    public ClienteExperiencia(HttpClient http) : base(http) { }

    public Task<List<ExperienciaVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<ExperienciaVista>>($"api/docente/{Escapar(cedula)}/experiecia");

    public Task CrearVariosAsync(int cedula, List<ExperienciaFormulario> renglones) =>
        PostAsync($"api/docente/{Escapar(cedula)}/experiecia", renglones);

    public Task<ExperienciaVista> ObtenerAsync(int id) => GetAsync<ExperienciaVista>($"api/experiecia/{Escapar(id)}");

    public Task ReemplazarAsync(int id, ExperienciaFormulario formulario) => PutAsync($"api/experiecia/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/experiecia/{Escapar(id)}");
}

/// <summary>Cliente HTTP de <c>/api/reconocimiento</c>.</summary>
public class ClienteReconocimiento : ClienteApiBase
{
    public ClienteReconocimiento(HttpClient http) : base(http) { }

    public Task<List<ReconocimientoVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<ReconocimientoVista>>($"api/docente/{Escapar(cedula)}/reconocimiento");

    public Task CrearVariosAsync(int cedula, List<ReconocimientoFormulario> renglones) =>
        PostAsync($"api/docente/{Escapar(cedula)}/reconocimiento", renglones);

    public Task<ReconocimientoVista> ObtenerAsync(int id) => GetAsync<ReconocimientoVista>($"api/reconocimiento/{Escapar(id)}");

    public Task ReemplazarAsync(int id, ReconocimientoFormulario formulario) => PutAsync($"api/reconocimiento/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/reconocimiento/{Escapar(id)}");
}
