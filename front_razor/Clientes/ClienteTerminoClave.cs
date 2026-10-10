using front_razor.Modelos;

namespace front_razor.Clientes;

/// <summary>Cliente HTTP de <c>/api/termino-clave</c>.</summary>
public class ClienteTerminoClave : ClienteApiBase
{
    public ClienteTerminoClave(HttpClient http) : base(http) { }

    public Task<List<TerminoClaveVista>> ListarAsync() => GetAsync<List<TerminoClaveVista>>("api/termino-clave");

    public Task<TerminoClaveVista> ObtenerAsync(string termino) => GetAsync<TerminoClaveVista>($"api/termino-clave/{Escapar(termino)}");

    public Task CrearAsync(TerminoClaveFormulario formulario) => PostAsync("api/termino-clave", formulario);

    public Task ReemplazarAsync(string termino, TerminoClaveFormulario formulario) => PutAsync($"api/termino-clave/{Escapar(termino)}", formulario);

    public Task EliminarAsync(string termino) => DeleteAsync($"api/termino-clave/{Escapar(termino)}");
}
