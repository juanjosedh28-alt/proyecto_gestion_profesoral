using ApiGestion.Modelos;

namespace ApiGestion.Servicios;

public interface IServicioPrograma
{
<<<<<<< HEAD
    Task<IEnumerable<Programa>> ListarAsync();
    Task<Programa?> ObtenerAsync(int id);
    Task<int> CrearAsync(Programa programa);
    Task<bool> ActualizarAsync(Programa programa);
    Task<bool> EliminarAsync(int id);
}
=======
    Task<List<Programa>> ObtenerTodosAsync(int limite = 100);
    Task<Programa> ObtenerPorIdAsync(int id);
    Task CrearAsync(Programa entidad);
    Task ActualizarAsync(Programa entidad);
    Task EliminarAsync(int id);
}
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
