<<<<<<< HEAD
using ApiGestion.Modelos;
=======
using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

<<<<<<< HEAD
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
=======
/// <summary>Contrato de las reglas de negocio de <c>programa</c>.</summary>
public interface IServicioPrograma
{
    Task<List<Programa>> ListarAsync();
    Task<Programa> ObtenerAsync(int clave);
    Task<Programa> CrearAsync(ProgramaCrear p);
    Task<Programa> ActualizarAsync(int id, ProgramaCrear p);
    Task EliminarAsync(int clave);
}

/// <summary>Servicio (capa de negocio) de <c>programa</c>: valida reglas, detecta conflictos y lanza excepciones que la API traduce a 404/409/422.</summary>
public class ServicioPrograma : IServicioPrograma
{
    private readonly IRepositorioPrograma _repo;

    public ServicioPrograma(IRepositorioPrograma repo) => _repo = repo;

    public Task<List<Programa>> ListarAsync() => _repo.ListarAsync();

    public async Task<Programa> ObtenerAsync(int clave) =>
        await _repo.ObtenerAsync(clave) ?? throw new NoEncontradoExcepcion($"No se encontró el programa con id {clave}.");

    public async Task<Programa> CrearAsync(ProgramaCrear p)
    {
        ValidarFechas(p.FechaCreacion, p.FechaCierre);
        if (await _repo.ExisteAsync(p.Id)) throw new ConflictoExcepcion($"Ya existe un programa con id {p.Id}.");
        var entidad = Mapear(p);
        await _repo.CrearAsync(entidad);
        return entidad;
    }

    public async Task<Programa> ActualizarAsync(int id, ProgramaCrear p)
    {
        ValidarFechas(p.FechaCreacion, p.FechaCierre);
        if (id != p.Id) throw new ValidacionExcepcion($"El id de la ruta ({id}) no coincide con el id del cuerpo ({p.Id}).");
        var entidad = Mapear(p);
        if (!await _repo.ActualizarAsync(entidad)) throw new NoEncontradoExcepcion($"No se encontró el programa con id {id}.");
        return entidad;
    }

    public async Task EliminarAsync(int clave)
    {
        if (!await _repo.EliminarLogicoAsync(clave)) throw new NoEncontradoExcepcion($"No se encontró el programa con id {clave}.");
    }

    /// <summary>Valida que las fechas existan de verdad y que el cierre no sea anterior a la creación.</summary>
    private static void ValidarFechas(string creacion, string? cierre)
    {
        const string formato = "yyyy-MM-dd";
        if (!DateTime.TryParseExact(creacion, formato, null, System.Globalization.DateTimeStyles.None, out var fc))
            throw new ValidacionExcepcion("Datos inválidos.", "fecha_creacion: La fecha de creación no es una fecha válida.");
        if (string.IsNullOrEmpty(cierre)) return;
        if (!DateTime.TryParseExact(cierre, formato, null, System.Globalization.DateTimeStyles.None, out var fcierre))
            throw new ValidacionExcepcion("Datos inválidos.", "fecha_cierre: La fecha de cierre no es una fecha válida.");
        if (fcierre < fc)
            throw new ValidacionExcepcion("Datos inválidos.", "fecha_cierre: La fecha de cierre no puede ser anterior a la fecha de creación.");
    }

    private static Programa Mapear(ProgramaCrear p) => new()
    {
            Id = p.Id,
            Nombre = p.Nombre,
            Tipo = p.Tipo,
            Nivel = p.Nivel,
            FechaCreacion = p.FechaCreacion,
            FechaCierre = p.FechaCierre,
            NumeroCohortes = p.NumeroCohortes,
            CantGraduados = p.CantGraduados,
            Ciudad = p.Ciudad,
            Facultad = p.Facultad,
            FechaActualizacion = DateTime.Now.ToString("yyyy-MM-dd"),
    };
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
}
