using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>termino_clave</c>.</summary>
public interface IRepositorioTerminoClave
{
    Task<List<TerminoClave>> ListarAsync();
    Task<TerminoClave?> ObtenerAsync(string clave);
    Task<bool> ExisteAsync(string clave);
    Task CrearAsync(TerminoClave entidad);
    Task<bool> ActualizarAsync(TerminoClave entidad);
    Task<bool> EliminarLogicoAsync(string clave);
}

/// <summary>Repositorio (capa de datos) de <c>termino_clave</c>. Solo habla SQL; todas las lecturas filtran <c>activo = 1</c>.</summary>
public class RepositorioTerminoClave : IRepositorioTerminoClave
{
    private readonly string _conexion;

    public RepositorioTerminoClave(IConfiguration config) => _conexion = config.GetConnectionString("SqlServer")!;

    private IDbConnection CrearConexion() => new SqlConnection(_conexion);

    public async Task<List<TerminoClave>> ListarAsync()
    {
        using var db = CrearConexion();
        return (await db.QueryAsync<TerminoClave>("SELECT termino, termino_ingles FROM termino_clave WHERE activo = 1 ORDER BY termino")).ToList();
    }

    public async Task<TerminoClave?> ObtenerAsync(string clave)
    {
        using var db = CrearConexion();
        return await db.QueryFirstOrDefaultAsync<TerminoClave>("SELECT termino, termino_ingles FROM termino_clave WHERE termino = @clave AND activo = 1", new { clave });
    }
    public async Task<bool> ExisteAsync(string clave)
    {
        // Incluye registros inactivos: la clave no se puede reutilizar (409).
        using var db = CrearConexion();
        return await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM termino_clave WHERE termino = @clave", new { clave }) > 0;
    }

    public async Task CrearAsync(TerminoClave entidad)
    {
        using var db = CrearConexion();
        var sql = @"INSERT INTO termino_clave (termino, termino_ingles, activo)
                    VALUES (@Termino, @TerminoIngles, 1)";
        await db.ExecuteAsync(sql, entidad);
    }

    public async Task<bool> ActualizarAsync(TerminoClave entidad)
    {
        using var db = CrearConexion();
        var sql = @"UPDATE termino_clave SET termino_ingles = @TerminoIngles
                    WHERE termino = @Termino AND activo = 1";
        return await db.ExecuteAsync(sql, entidad) > 0;
    }

    public async Task<bool> EliminarLogicoAsync(string clave)
    {
        using var db = CrearConexion();
        return await db.ExecuteAsync("UPDATE termino_clave SET activo = 0 WHERE termino = @clave AND activo = 1", new { clave }) > 0;
    }
}
