using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

public interface IRepositorioPrograma
{
<<<<<<< HEAD
    Task<IEnumerable<Programa>> ListarAsync();
    Task<Programa?> ObtenerAsync(int id);
    Task<bool> ExisteAsync(int id);
    Task<int> CrearAsync(Programa programa);
    Task<bool> ActualizarAsync(Programa programa);
    Task<bool> EliminarAsync(int id);
}
=======
    Task<List<Programa>> ObtenerTodosAsync(int limite = 100);
    Task<Programa?> ObtenerPorIdAsync(int id);
    Task CrearAsync(Programa entidad);
    Task<bool> ActualizarAsync(Programa entidad);
    Task<bool> EliminarLogicoAsync(int id);
}
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
