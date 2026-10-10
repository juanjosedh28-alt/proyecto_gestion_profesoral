using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>programa</c>.</summary>
public interface IRepositorioPrograma
{
    Task<List<Programa>> ListarAsync();
    Task<Programa?> ObtenerAsync(int clave);
    Task<bool> ExisteAsync(int clave);
    Task CrearAsync(Programa entidad);
    Task<bool> ActualizarAsync(Programa entidad);
    Task<bool> EliminarLogicoAsync(int clave);
}

/// <summary>Repositorio (capa de datos) de <c>programa</c>. Solo habla SQL; todas las lecturas filtran <c>activo = 1</c>.</summary>
public class RepositorioPrograma : IRepositorioPrograma
{
    private readonly string _conexion;

    public RepositorioPrograma(IConfiguration config) => _conexion = config.GetConnectionString("SqlServer")!;

    private IDbConnection CrearConexion() => new SqlConnection(_conexion);

    public async Task<List<Programa>> ListarAsync()
    {
        using var db = CrearConexion();
        return (await db.QueryAsync<Programa>("SELECT id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes, cant_graduados, ciudad, facultad, fecha_actualizacion FROM programa WHERE activo = 1 ORDER BY id")).ToList();
    }

    public async Task<Programa?> ObtenerAsync(int clave)
    {
        using var db = CrearConexion();
        return await db.QueryFirstOrDefaultAsync<Programa>("SELECT id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes, cant_graduados, ciudad, facultad, fecha_actualizacion FROM programa WHERE id = @clave AND activo = 1", new { clave });
    }
    public async Task<bool> ExisteAsync(int clave)
    {
        // Incluye registros inactivos: la clave no se puede reutilizar (409).
        using var db = CrearConexion();
        return await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM programa WHERE id = @clave", new { clave }) > 0;
    }

    public async Task CrearAsync(Programa entidad)
    {
        using var db = CrearConexion();
        var sql = @"INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, numero_cohortes, cant_graduados, ciudad, facultad, fecha_actualizacion, activo)
                    VALUES (@Id, @Nombre, @Tipo, @Nivel, @FechaCreacion, @FechaCierre, @NumeroCohortes, @CantGraduados, @Ciudad, @Facultad, @FechaActualizacion, 1)";
        await db.ExecuteAsync(sql, entidad);
    }

    public async Task<bool> ActualizarAsync(Programa entidad)
    {
        using var db = CrearConexion();
        var sql = @"UPDATE programa SET nombre = @Nombre, tipo = @Tipo, nivel = @Nivel, fecha_creacion = @FechaCreacion, fecha_cierre = @FechaCierre, numero_cohortes = @NumeroCohortes, cant_graduados = @CantGraduados, ciudad = @Ciudad, facultad = @Facultad, fecha_actualizacion = @FechaActualizacion
                    WHERE id = @Id AND activo = 1";
        return await db.ExecuteAsync(sql, entidad) > 0;
    }

    public async Task<bool> EliminarLogicoAsync(int clave)
    {
        using var db = CrearConexion();
        return await db.ExecuteAsync("UPDATE programa SET activo = 0 WHERE id = @clave AND activo = 1", new { clave }) > 0;
    }
}
