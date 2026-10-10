using ApiGestion.Modelos;

namespace ApiGestion.Servicios;

public interface IServicioPrograma
{
    Task<IEnumerable<Programa>> ListarAsync();
    Task<Programa?> ObtenerAsync(int id);
    Task<int> CrearAsync(Programa programa);
    Task<bool> ActualizarAsync(Programa programa);
    Task<bool> EliminarAsync(int id);
}
