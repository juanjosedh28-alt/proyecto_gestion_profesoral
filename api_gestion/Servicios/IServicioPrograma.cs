using ApiGestion.Modelos;

namespace ApiGestion.Servicios;

public interface IServicioPrograma
{
    Task<List<Programa>> ObtenerTodosAsync(int limite = 100);
    Task<Programa> ObtenerPorIdAsync(int id);
    Task CrearAsync(Programa entidad);
    Task ActualizarAsync(Programa entidad);
    Task EliminarAsync(int id);
}