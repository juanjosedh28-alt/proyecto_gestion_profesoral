using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>linea_investigacion</c>.</summary>
public interface IRepositorioLineaInvestigacion
{
    Task<List<LineaInvestigacion>> ListarAsync();
    Task<LineaInvestigacion?> ObtenerAsync(int clave);
    Task<bool> ExisteAsync(int clave);
    Task<int> CrearAsync(LineaInvestigacion entidad);
    Task<bool> ActualizarAsync(LineaInvestigacion entidad);
    Task<bool> EliminarLogicoAsync(int clave);
}

/// <summary>Repositorio (capa de datos) de <c>linea_investigacion</c>. Solo habla SQL; todas las lecturas filtran <c>activo = 1</c>.</summary>
public class RepositorioLineaInvestigacion : IRepositorioLineaInvestigacion
{
    private readonly string _conexion;

    public RepositorioLineaInvestigacion(IConfiguration config) => _conexion = config.GetConnectionString("SqlServer")!;

    private IDbConnection CrearConexion() => new SqlConnection(_conexion);

    public async Task<List<LineaInvestigacion>> ListarAsync()
    {
        using var db = CrearConexion();
        return (await db.QueryAsync<LineaInvestigacion>("SELECT id, nombre, descripcion FROM linea_investigacion WHERE activo = 1 ORDER BY id")).ToList();
    }

    public async Task<LineaInvestigacion?> ObtenerAsync(int clave)
    {
        using var db = CrearConexion();
        return await db.QueryFirstOrDefaultAsync<LineaInvestigacion>("SELECT id, nombre, descripcion FROM linea_investigacion WHERE id = @clave AND activo = 1", new { clave });
    }
    public async Task<bool> ExisteAsync(int clave)
    {
        // Incluye registros inactivos: la clave no se puede reutilizar (409).
        using var db = CrearConexion();
        return await db.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM linea_investigacion WHERE id = @clave", new { clave }) > 0;
    }

    public async Task<int> CrearAsync(LineaInvestigacion entidad)
    {
        using var db = CrearConexion();
        var sql = @"INSERT INTO linea_investigacion (nombre, descripcion, activo)
                    OUTPUT INSERTED.id
                    VALUES (@Nombre, @Descripcion, 1)";
        return await db.QuerySingleAsync<int>(sql, entidad);
    }

    public async Task<bool> ActualizarAsync(LineaInvestigacion entidad)
    {
        using var db = CrearConexion();
        var sql = @"UPDATE linea_investigacion SET nombre = @Nombre, descripcion = @Descripcion
                    WHERE id = @Id AND activo = 1";
        return await db.ExecuteAsync(sql, entidad) > 0;
    }

    public async Task<bool> EliminarLogicoAsync(int clave)
    {
        using var db = CrearConexion();
        return await db.ExecuteAsync("UPDATE linea_investigacion SET activo = 0 WHERE id = @clave AND activo = 1", new { clave }) > 0;
    }
}
