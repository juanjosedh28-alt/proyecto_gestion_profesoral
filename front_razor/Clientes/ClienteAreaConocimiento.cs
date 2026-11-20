using front_razor.Modelos;

namespace front_razor.Clientes;

/// <summary>
/// Cliente HTTP de <c>/api/area-conocimiento</c>. En la v1 era solo de lectura;
/// desde la v2 también crea, edita y retira (D65).
/// </summary>
public class ClienteAreaConocimiento : ClienteApiBase
{
    public ClienteAreaConocimiento(HttpClient http) : base(http) { }

    public Task<List<AreaConocimientoVista>> ListarAsync() => GetAsync<List<AreaConocimientoVista>>("api/area-conocimiento");

    public Task<AreaConocimientoVista> ObtenerAsync(string id) => GetAsync<AreaConocimientoVista>($"api/area-conocimiento/{Escapar(id)}");

    public Task CrearAsync(AreaConocimientoFormulario formulario) => PostAsync("api/area-conocimiento", formulario);

    public Task ReemplazarAsync(string id, AreaConocimientoFormulario formulario) => PutAsync($"api/area-conocimiento/{Escapar(id)}", formulario);

    public Task EliminarAsync(string id) => DeleteAsync($"api/area-conocimiento/{Escapar(id)}");
}
