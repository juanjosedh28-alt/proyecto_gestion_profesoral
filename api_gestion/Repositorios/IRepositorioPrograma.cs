using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

public interface IRepositorioPrograma
{
    Task<IEnumerable<Programa>> ListarAsync();
    Task<Programa?> ObtenerAsync(int id);
    Task<bool> ExisteAsync(int id);
    Task<int> CrearAsync(Programa programa);
    Task<bool> ActualizarAsync(Programa programa);
    Task<bool> EliminarAsync(int id);
}
