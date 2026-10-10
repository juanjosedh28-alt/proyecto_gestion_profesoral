using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;
using ApiGestion.Modelos;

namespace ApiGestion.Repositorios;

/// <summary>Contrato de acceso a datos de <c>area_conocimiento</c>.</summary>
public interface IRepositorioAreaConocimiento
{
    Task<List<AreaConocimiento>> ListarAsync();
    Task<AreaConocimiento?> ObtenerAsync(string clave);
}

/// <summary>Repositorio (capa de datos) de <c>area_conocimiento</c>. Solo habla SQL; todas las lecturas filtran <c>activo = 1</c>.</summary>
public class RepositorioAreaConocimiento : IRepositorioAreaConocimiento
{
    private readonly string _conexion;

    public RepositorioAreaConocimiento(IConfiguration config) => _conexion = config.GetConnectionString("SqlServer")!;

    private IDbConnection CrearConexion() => new SqlConnection(_conexion);

    public async Task<List<AreaConocimiento>> ListarAsync()
    {
        using var db = CrearConexion();
        return (await db.QueryAsync<AreaConocimiento>("SELECT id, gran_area, area, disciplina FROM area_conocimiento WHERE activo = 1 ORDER BY id")).ToList();
    }

    public async Task<AreaConocimiento?> ObtenerAsync(string clave)
    {
        using var db = CrearConexion();
        return await db.QueryFirstOrDefaultAsync<AreaConocimiento>("SELECT id, gran_area, area, disciplina FROM area_conocimiento WHERE id = @clave AND activo = 1", new { clave });
    }
}
