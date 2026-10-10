using front_razor.Modelos;

namespace front_razor.Clientes;

/// <summary>Cliente HTTP de <c>/api/programa</c>.</summary>
public class ClientePrograma : ClienteApiBase
{
    public ClientePrograma(HttpClient http) : base(http) { }

    public Task<List<ProgramaVista>> ListarAsync() => GetAsync<List<ProgramaVista>>("api/programa");

    public Task<ProgramaVista> ObtenerAsync(int id) => GetAsync<ProgramaVista>($"api/programa/{Escapar(id)}");

    public Task CrearAsync(ProgramaFormulario formulario) => PostAsync("api/programa", formulario);

    public Task ReemplazarAsync(int id, ProgramaFormulario formulario) => PutAsync($"api/programa/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/programa/{Escapar(id)}");
}
