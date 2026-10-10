using front_razor.Modelos;

namespace front_razor.Clientes;

/// <summary>Cliente HTTP de <c>/api/red</c>.</summary>
public class ClienteRed : ClienteApiBase
{
    public ClienteRed(HttpClient http) : base(http) { }

    public Task<List<RedVista>> ListarAsync() => GetAsync<List<RedVista>>("api/red");

    public Task<RedVista> ObtenerAsync(int idr) => GetAsync<RedVista>($"api/red/{Escapar(idr)}");

    public Task CrearAsync(RedFormulario formulario) => PostAsync("api/red", formulario);

    public Task ReemplazarAsync(int idr, RedFormulario formulario) => PutAsync($"api/red/{Escapar(idr)}", formulario);

    public Task EliminarAsync(int idr) => DeleteAsync($"api/red/{Escapar(idr)}");
}
