using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

public class ServicioPrograma : IServicioPrograma
{
    private readonly IRepositorioPrograma _repo;

    public ServicioPrograma(IRepositorioPrograma repo)
    {
        _repo = repo;
    }

    public Task<List<Programa>> ObtenerTodosAsync(int limite = 100) => _repo.ObtenerTodosAsync(limite);

    public async Task<Programa> ObtenerPorIdAsync(int id)
    {
        var item = await _repo.ObtenerPorIdAsync(id);
        if (item == null) throw new NoEncontradoExcepcion($"El programa con ID {id} no existe o está inactivo.");
        return item;
    }

    public Task CrearAsync(Programa entidad) => _repo.CrearAsync(entidad);

    public async Task ActualizarAsync(Programa entidad)
    {
        var exito = await _repo.ActualizarAsync(entidad);
        if (!exito) throw new NoEncontradoExcepcion($"No se pudo actualizar: el programa con ID {entidad.Id} no existe.");
    }

    public async Task EliminarAsync(int id)
    {
        var exito = await _repo.EliminarLogicoAsync(id);
        if (!exito) throw new NoEncontradoExcepcion($"El programa con ID {id} no existe o ya fue eliminado.");
    }
}