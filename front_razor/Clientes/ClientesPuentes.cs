using front_razor.Modelos;

namespace front_razor.Clientes;

// ===================================================================
// Clientes HTTP de las TABLAS PUENTE y del detalle 1:1 del estudio.
// En las puentes, la clave son DOS valores: por eso Obtener, Reemplazar y
// Eliminar reciben las dos (por ejemplo, docente y programa).
// ===================================================================

/// <summary>Cliente HTTP de <c>/api/docente-departamento</c> (docente ↔ programa).</summary>
public class ClienteDocenteDepartamento : ClienteApiBase
{
    public ClienteDocenteDepartamento(HttpClient http) : base(http) { }

    public Task<List<DocenteDepartamentoVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<DocenteDepartamentoVista>>($"api/docente/{Escapar(cedula)}/docente-departamento");

    public Task<DocenteDepartamentoVista> ObtenerAsync(int docente, int departamento) =>
        GetAsync<DocenteDepartamentoVista>($"api/docente-departamento/{Escapar(docente)}/{Escapar(departamento)}");

    public Task CrearAsync(DocenteDepartamentoFormulario formulario) => PostAsync("api/docente-departamento", formulario);

    public Task ReemplazarAsync(int docente, int departamento, DocenteDepartamentoFormulario formulario) =>
        PutAsync($"api/docente-departamento/{Escapar(docente)}/{Escapar(departamento)}", formulario);

    public Task EliminarAsync(int docente, int departamento) =>
        DeleteAsync($"api/docente-departamento/{Escapar(docente)}/{Escapar(departamento)}");
}

/// <summary>Cliente HTTP de <c>/api/red-docente</c> (red ↔ docente).</summary>
public class ClienteRedDocente : ClienteApiBase
{
    public ClienteRedDocente(HttpClient http) : base(http) { }

    public Task<List<RedDocenteVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<RedDocenteVista>>($"api/docente/{Escapar(cedula)}/red-docente");

    public Task<RedDocenteVista> ObtenerAsync(int red, int docente) =>
        GetAsync<RedDocenteVista>($"api/red-docente/{Escapar(red)}/{Escapar(docente)}");

    public Task CrearAsync(RedDocenteFormulario formulario) => PostAsync("api/red-docente", formulario);

    public Task ReemplazarAsync(int red, int docente, RedDocenteFormulario formulario) =>
        PutAsync($"api/red-docente/{Escapar(red)}/{Escapar(docente)}", formulario);

    public Task EliminarAsync(int red, int docente) =>
        DeleteAsync($"api/red-docente/{Escapar(red)}/{Escapar(docente)}");
}

/// <summary>Cliente HTTP de <c>/api/intereses-futuros</c> (docente ↔ término clave). Sin editar: puente puro.</summary>
public class ClienteInteresFuturo : ClienteApiBase
{
    public ClienteInteresFuturo(HttpClient http) : base(http) { }

    public Task<List<InteresFuturoVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<InteresFuturoVista>>($"api/docente/{Escapar(cedula)}/intereses-futuros");

    public Task CrearAsync(InteresFuturoFormulario formulario) => PostAsync("api/intereses-futuros", formulario);

    // El término es texto: Escapar evita que un espacio o una tilde rompan la ruta.
    public Task EliminarAsync(int docente, string termino) =>
        DeleteAsync($"api/intereses-futuros/{Escapar(docente)}/{Escapar(termino)}");
}

/// <summary>Cliente HTTP de <c>/api/estudio-ac</c> (estudio ↔ área de conocimiento). Sin editar: puente puro.</summary>
public class ClienteEstudioAc : ClienteApiBase
{
    public ClienteEstudioAc(HttpClient http) : base(http) { }

    public Task<List<EstudioAcVista>> ListarPorEstudioAsync(int estudio) =>
        GetAsync<List<EstudioAcVista>>($"api/estudios-realizados/{Escapar(estudio)}/estudio-ac");

    public Task CrearAsync(EstudioAcFormulario formulario) => PostAsync("api/estudio-ac", formulario);

    public Task EliminarAsync(int estudio, string area) =>
        DeleteAsync($"api/estudio-ac/{Escapar(estudio)}/{Escapar(area)}");
}

/// <summary>Cliente HTTP de <c>/api/apoyo-profesoral</c> (1:1 con el estudio).</summary>
public class ClienteApoyoProfesoral : ClienteApiBase
{
    public ClienteApoyoProfesoral(HttpClient http) : base(http) { }

    /// <summary>null si el estudio todavía no tiene apoyo (la API responde 404).</summary>
    public Task<ApoyoProfesoralVista?> ObtenerAsync(int estudios) =>
        GetOpcionalAsync<ApoyoProfesoralVista>($"api/apoyo-profesoral/{Escapar(estudios)}");

    public Task CrearAsync(ApoyoProfesoralFormulario formulario) => PostAsync("api/apoyo-profesoral", formulario);

    public Task ReemplazarAsync(int estudios, ApoyoProfesoralFormulario formulario) =>
        PutAsync($"api/apoyo-profesoral/{Escapar(estudios)}", formulario);

    public Task EliminarAsync(int estudios) => DeleteAsync($"api/apoyo-profesoral/{Escapar(estudios)}");
}

/// <summary>Cliente HTTP de <c>/api/beca</c> (1:1 con el estudio).</summary>
public class ClienteBeca : ClienteApiBase
{
    public ClienteBeca(HttpClient http) : base(http) { }

    /// <summary>null si el estudio todavía no tiene beca (la API responde 404).</summary>
    public Task<BecaVista?> ObtenerAsync(int estudios) => GetOpcionalAsync<BecaVista>($"api/beca/{Escapar(estudios)}");

    public Task CrearAsync(BecaFormulario formulario) => PostAsync("api/beca", formulario);

    public Task ReemplazarAsync(int estudios, BecaFormulario formulario) => PutAsync($"api/beca/{Escapar(estudios)}", formulario);

    public Task EliminarAsync(int estudios) => DeleteAsync($"api/beca/{Escapar(estudios)}");
}
