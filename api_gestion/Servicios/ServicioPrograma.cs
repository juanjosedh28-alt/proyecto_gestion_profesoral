using ApiGestion.Excepciones;
using ApiGestion.Modelos;
using ApiGestion.Peticiones;
using ApiGestion.Repositorios;

namespace ApiGestion.Servicios;

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
        // Si el id ya existe, el procedimiento sp_crear_programa responde 409.
        return await _repo.CrearAsync(Mapear(p));
    }

    public async Task<Programa> ActualizarAsync(int id, ProgramaCrear p)
    {
        ValidarFechas(p.FechaCreacion, p.FechaCierre);
        if (id != p.Id) throw new ValidacionExcepcion($"El id de la ruta ({id}) no coincide con el id del cuerpo ({p.Id}).");
        return await _repo.ActualizarAsync(Mapear(p));   // 404 lo responde el procedimiento
    }

    public Task EliminarAsync(int clave) => _repo.EliminarLogicoAsync(clave);

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
    };
}
