using front_razor.Modelos;

namespace front_razor.Clientes;

/// <summary>Cliente HTTP de <c>/api/linea-investigacion</c>.</summary>
public class ClienteLineaInvestigacion : ClienteApiBase
{
    public ClienteLineaInvestigacion(HttpClient http) : base(http) { }

    public Task<List<LineaInvestigacionVista>> ListarAsync() => GetAsync<List<LineaInvestigacionVista>>("api/linea-investigacion");

    public Task<LineaInvestigacionVista> ObtenerAsync(int id) => GetAsync<LineaInvestigacionVista>($"api/linea-investigacion/{Escapar(id)}");

    public Task CrearAsync(LineaInvestigacionFormulario formulario) => PostAsync("api/linea-investigacion", formulario);

    public Task ReemplazarAsync(int id, LineaInvestigacionFormulario formulario) => PutAsync($"api/linea-investigacion/{Escapar(id)}", formulario);

    public Task EliminarAsync(int id) => DeleteAsync($"api/linea-investigacion/{Escapar(id)}");
}
