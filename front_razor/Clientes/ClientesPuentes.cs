using front_razor.Modelos;

namespace front_razor.Clientes;

public class ClienteDocenteDepartamento : ClienteApiBase
{
    public ClienteDocenteDepartamento(HttpClient http) : base(http) { }

    public Task<List<DocenteDepartamentoVista>> ListarAsync() => GetAsync<List<DocenteDepartamentoVista>>("api/docente-departamento");

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

public class ClienteRedDocente : ClienteApiBase
{
    public ClienteRedDocente(HttpClient http) : base(http) { }

    public Task<List<RedDocenteVista>> ListarAsync() => GetAsync<List<RedDocenteVista>>("api/red-docente");

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

public class ClienteInteresFuturo : ClienteApiBase
{
    public ClienteInteresFuturo(HttpClient http) : base(http) { }

    public Task<List<InteresFuturoVista>> ListarAsync() => GetAsync<List<InteresFuturoVista>>("api/intereses-futuros");

    public Task<List<InteresFuturoVista>> ListarPorDocenteAsync(int cedula) =>
        GetAsync<List<InteresFuturoVista>>($"api/docente/{Escapar(cedula)}/intereses-futuros");

    public Task CrearAsync(InteresFuturoFormulario formulario) => PostAsync("api/intereses-futuros", formulario);

    public Task EliminarAsync(int docente, string termino) =>
        DeleteAsync($"api/intereses-futuros/{Escapar(docente)}/{Escapar(termino)}");
}

public class ClienteEstudioAc : ClienteApiBase
{
    public ClienteEstudioAc(HttpClient http) : base(http) { }

    public Task<List<EstudioAcVista>> ListarAsync() => GetAsync<List<EstudioAcVista>>("api/estudio-ac");

    public Task<List<EstudioAcVista>> ListarPorEstudioAsync(int estudio) =>
        GetAsync<List<EstudioAcVista>>($"api/estudios-realizados/{Escapar(estudio)}/estudio-ac");

    public Task CrearAsync(EstudioAcFormulario formulario) => PostAsync("api/estudio-ac", formulario);

    public Task EliminarAsync(int estudio, string area) =>
        DeleteAsync($"api/estudio-ac/{Escapar(estudio)}/{Escapar(area)}");
}

public class ClienteApoyoProfesoral : ClienteApiBase
{
    public ClienteApoyoProfesoral(HttpClient http) : base(http) { }

    public Task<List<ApoyoProfesoralVista>> ListarAsync() => GetAsync<List<ApoyoProfesoralVista>>("api/apoyo-profesoral");

    public Task<ApoyoProfesoralVista?> ObtenerAsync(int estudios) =>
        GetOpcionalAsync<ApoyoProfesoralVista>($"api/apoyo-profesoral/{Escapar(estudios)}");

    public Task CrearAsync(ApoyoProfesoralFormulario formulario) => PostAsync("api/apoyo-profesoral", formulario);

    public Task ReemplazarAsync(int estudios, ApoyoProfesoralFormulario formulario) =>
        PutAsync($"api/apoyo-profesoral/{Escapar(estudios)}", formulario);

    public Task EliminarAsync(int estudios) => DeleteAsync($"api/apoyo-profesoral/{Escapar(estudios)}");
}

public class ClienteBeca : ClienteApiBase
{
    public ClienteBeca(HttpClient http) : base(http) { }

    public Task<List<BecaVista>> ListarAsync() => GetAsync<List<BecaVista>>("api/beca");

    public Task<BecaVista?> ObtenerAsync(int estudios) => GetOpcionalAsync<BecaVista>($"api/beca/{Escapar(estudios)}");

    public Task CrearAsync(BecaFormulario formulario) => PostAsync("api/beca", formulario);

    public Task ReemplazarAsync(int estudios, BecaFormulario formulario) => PutAsync($"api/beca/{Escapar(estudios)}", formulario);

    public Task EliminarAsync(int estudios) => DeleteAsync($"api/beca/{Escapar(estudios)}");
}
