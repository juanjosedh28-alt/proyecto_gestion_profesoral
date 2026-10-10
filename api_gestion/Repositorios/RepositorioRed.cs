using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>red</c>.</summary>
public interface IRepositorioRed
{
    Task<List<Red>> ListarAsync();
    Task<Red?> ObtenerAsync(int clave);
    Task<bool> ExisteAsync(int clave);
    Task CrearAsync(Red entidad);
    Task<bool> ActualizarAsync(Red entidad);
    Task<bool> EliminarLogicoAsync(int clave);
}

/// <summary>Repositorio (capa de datos) de <c>red</c>. Solo habla SQL; todas las lecturas filtran <c>activo = 1</c>.</summary>
public class RepositorioRed : IRepositorioRed
{
    private readonly string _conexion;

    public RepositorioRed(IConfiguration config) => _conexion = config.GetConnectionString("SqlServer")!;

    private IDbConnection CrearConexion() => new SqlConnection(_conexion);

    public async Task<List<Red>> ListarAsync()
    {
        using var db = CrearConexion();
        return (await db.QueryAsync<Red>("SELECT idr, nombre, url, pais FROM red WHERE activo = 1 ORDER BY idr")).ToList();
    }

    public async Task<Red?> ObtenerAsync(int clave)
    {
        using var db = CrearConexion();
        return await db.QueryFirstOrDefaultAsync<Red>("SELECT idr, nombre, url, pais FROM red WHERE idr = @clave AND activo = 1", new { clave });
    }
    public async Task<bool> ExisteAsync(int clave)
    {
        // Incluye registros inactivos: la clave no se puede reutilizar (409).
        using var db = CrearConexion();
        return await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM red WHERE idr = @clave", new { clave }) > 0;
    }

    public async Task CrearAsync(Red entidad)
    {
        using var db = CrearConexion();
        var sql = @"INSERT INTO red (idr, nombre, url, pais, activo)
                    VALUES (@Idr, @Nombre, @Url, @Pais, 1)";
        await db.ExecuteAsync(sql, entidad);
    }

    public async Task<bool> ActualizarAsync(Red entidad)
    {
        using var db = CrearConexion();
        var sql = @"UPDATE red SET nombre = @Nombre, url = @Url, pais = @Pais
                    WHERE idr = @Idr AND activo = 1";
        return await db.ExecuteAsync(sql, entidad) > 0;
    }

    public async Task<bool> EliminarLogicoAsync(int clave)
    {
        using var db = CrearConexion();
        return await db.ExecuteAsync("UPDATE red SET activo = 0 WHERE idr = @clave AND activo = 1", new { clave }) > 0;
    }
}
