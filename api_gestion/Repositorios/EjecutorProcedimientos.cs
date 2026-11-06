using System.Data;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiGestion.Repositorios;

/// <summary>
/// Único punto de la API que habla con SQL Server (v2).
///
/// QUÉ HACE: abre una conexión, ejecuta un PROCEDIMIENTO ALMACENADO por su nombre
/// con sus parámetros, y cierra la conexión.
///
/// POR QUÉ ASÍ: la v2 exige que todo el SQL viva en la base de datos
/// (db/v2_procedimientos.sql). Aquí nunca se escribe una consulta SQL:
/// solo se dice "ejecute sp_crear_docente con estos datos". El tipo de comando
/// es <see cref="CommandType.StoredProcedure"/>, y los valores viajan como
/// PARÁMETROS (no pegados en un texto), que es lo que impide la inyección de SQL.
///
/// Los parámetros se pasan como un objeto anónimo: <c>new { cedula = 1017245 }</c>
/// se convierte en <c>@cedula = 1017245</c>. Dapper además convierte cada columna
/// que devuelve el procedimiento (docente_nombre) en la propiedad del modelo (DocenteNombre).
///
/// SI FALLA: si el procedimiento hace THROW 50001..50004 (o la base rechaza algo),
/// aquí sale una <see cref="SqlException"/> que el middleware ManejadorErroresSql
/// convierte en 404/409/422 con el mensaje en castellano.
/// </summary>
public interface IEjecutorProcedimientos
{
    /// <summary>Ejecuta el procedimiento y devuelve todas las filas.</summary>
    Task<List<T>> ListarAsync<T>(string procedimiento, object? parametros = null);

    /// <summary>Ejecuta el procedimiento y devuelve la primera fila, o null si no devolvió ninguna.</summary>
    Task<T?> PrimeroAsync<T>(string procedimiento, object? parametros = null);

    /// <summary>Ejecuta el procedimiento cuando no devuelve filas (por ejemplo, sp_eliminar_).</summary>
    Task EjecutarAsync(string procedimiento, object? parametros = null);
}

/// <inheritdoc cref="IEjecutorProcedimientos"/>
public class EjecutorProcedimientos : IEjecutorProcedimientos
{
    private readonly string _conexion;

    // La cadena de conexión llega por variable de entorno (ConnectionStrings__SqlServer en docker-compose).
    public EjecutorProcedimientos(IConfiguration config) => _conexion = config.GetConnectionString("SqlServer")!;

    public async Task<List<T>> ListarAsync<T>(string procedimiento, object? parametros = null)
    {
        await using var conexion = new SqlConnection(_conexion);
        var filas = await conexion.QueryAsync<T>(procedimiento, parametros, commandType: CommandType.StoredProcedure);
        return filas.ToList();
    }

    public async Task<T?> PrimeroAsync<T>(string procedimiento, object? parametros = null)
    {
        await using var conexion = new SqlConnection(_conexion);
        return await conexion.QueryFirstOrDefaultAsync<T>(procedimiento, parametros, commandType: CommandType.StoredProcedure);
    }

    public async Task EjecutarAsync(string procedimiento, object? parametros = null)
    {
        await using var conexion = new SqlConnection(_conexion);
        await conexion.ExecuteAsync(procedimiento, parametros, commandType: CommandType.StoredProcedure);
    }
}
