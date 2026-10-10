using ApiGestion.Modelos;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

public class ServicioPrograma : IServicioPrograma
{
    private readonly IRepositorioPrograma _repositorio;

    public ServicioPrograma(IRepositorioPrograma repositorio)
    {
        _repositorio = repositorio;
    }

    public Task<IEnumerable<Programa>> ListarAsync() => _repositorio.ListarAsync();
    public Task<Programa?> ObtenerAsync(int id) => _repositorio.ObtenerAsync(id);
    public Task<int> CrearAsync(Programa programa) => _repositorio.CrearAsync(programa);
    public Task<bool> ActualizarAsync(Programa programa) => _repositorio.ActualizarAsync(programa);
    public Task<bool> EliminarAsync(int id) => _repositorio.EliminarAsync(id);
}
