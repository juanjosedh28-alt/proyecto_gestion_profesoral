using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

/// <summary>Contrato de las reglas de negocio de <c>area_conocimiento</c>.</summary>
public interface IServicioAreaConocimiento
{
    Task<List<AreaConocimiento>> ListarAsync();
    Task<AreaConocimiento> ObtenerAsync(string clave);
    Task<AreaConocimiento> CrearAsync(AreaConocimientoCrear p);
    Task<AreaConocimiento> ActualizarAsync(string clave, AreaConocimientoCrear p);
    Task EliminarAsync(string clave);
}

/// <summary>
/// Servicio (capa de negocio) de <c>area_conocimiento</c>.
/// En la v1 el catálogo era de solo lectura (D22). La v2 le da CRUD completo
/// (D65): VERSION_2.md pide crear, editar y retirar en TODAS las tablas de la v1.
/// Los 409 (código repetido) y 404 (no existe) los responde el procedimiento.
/// </summary>
public class ServicioAreaConocimiento : IServicioAreaConocimiento
{
    private readonly IRepositorioAreaConocimiento _repo;

    public ServicioAreaConocimiento(IRepositorioAreaConocimiento repo) => _repo = repo;

    public Task<List<AreaConocimiento>> ListarAsync() => _repo.ListarAsync();

    public async Task<AreaConocimiento> ObtenerAsync(string clave) =>
        await _repo.ObtenerAsync(clave) ?? throw new NoEncontradoExcepcion($"No se encontró el área de conocimiento con id {clave}.");

    // El código se guarda en mayúsculas («1a01» y «1A01» son el mismo código).
    public Task<AreaConocimiento> CrearAsync(AreaConocimientoCrear p) => _repo.CrearAsync(Mapear(p));

    public Task<AreaConocimiento> ActualizarAsync(string clave, AreaConocimientoCrear p)
    {
        if (!string.Equals(clave, p.Id, StringComparison.OrdinalIgnoreCase))
            throw new ValidacionExcepcion($"El código de la ruta ({clave}) no coincide con el código del cuerpo ({p.Id}).");
        return _repo.ActualizarAsync(Mapear(p));
    }

    // Borrado LÓGICO (activo = 0), como en todas las tablas del módulo.
    public Task EliminarAsync(string clave) => _repo.EliminarLogicoAsync(clave);

    private static AreaConocimiento Mapear(AreaConocimientoCrear p) => new()
    {
        Id = p.Id.Trim().ToUpperInvariant(),
        GranArea = p.GranArea.Trim(),
        Area = p.Area.Trim(),
        Disciplina = p.Disciplina.Trim(),
    };
}
