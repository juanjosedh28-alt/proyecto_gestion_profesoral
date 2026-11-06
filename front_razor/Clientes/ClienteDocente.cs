using front_razor.Modelos;

namespace front_razor.Clientes;

/// <summary>Cliente HTTP de <c>/api/docente</c> (el maestro del módulo).</summary>
public class ClienteDocente : ClienteApiBase
{
    public ClienteDocente(HttpClient http) : base(http) { }

    /// <summary>Solo los docentes ACTIVOS: con esto se llenan los desplegables.</summary>
    public Task<List<DocenteVista>> ListarAsync() => GetAsync<List<DocenteVista>>("api/docente");

    public Task<DocenteVista> ObtenerAsync(int cedula) => GetAsync<DocenteVista>($"api/docente/{Escapar(cedula)}");

    public Task CrearAsync(DocenteFormulario formulario) => PostAsync("api/docente", formulario);

    public Task ReemplazarAsync(int cedula, DocenteFormulario formulario) => PutAsync($"api/docente/{Escapar(cedula)}", formulario);

    public Task EliminarAsync(int cedula) => DeleteAsync($"api/docente/{Escapar(cedula)}");
}
