using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

public interface IRepositorioPrograma
{
    Task<List<Programa>> ObtenerTodosAsync(int limite = 100);
    Task<Programa?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Programa entidad);
    Task<bool> ActualizarAsync(Programa entidad);
    Task<bool> EliminarLogicoAsync(int id);
}