using front_razor.Modelos;

namespace front_razor.Clientes;

/// <summary>Cliente HTTP de solo lectura de <c>/api/area-conocimiento</c>.</summary>
public class ClienteAreaConocimiento : ClienteApiBase
{
    public ClienteAreaConocimiento(HttpClient http) : base(http) { }

    public Task<List<AreaConocimientoVista>> ListarAsync() => GetAsync<List<AreaConocimientoVista>>("api/area-conocimiento");

    public Task<AreaConocimientoVista> ObtenerAsync(string id) => GetAsync<AreaConocimientoVista>($"api/area-conocimiento/{Escapar(id)}");
}
