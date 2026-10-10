using ApiGestion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;

namespace ApiGestion.Repositorios;

public class RepositorioProgramaSqlServer : IRepositorioPrograma
{
    private readonly string _cadenaConexion;

    public RepositorioProgramaSqlServer(IConfiguration configuracion)
    {
        _cadenaConexion = configuracion.GetConnectionString("DefaultConnection") 
            ?? "Server=sqlserver,1433;Database=gestion_profesoral;User Id=sa;Password=Aplicacionweb123!;TrustServerCertificate=True;";
    }

    public async Task<IEnumerable<Programa>> ListarAsync()
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryAsync<Programa>("SELECT * FROM programa WHERE activo = 1");
    }

    public async Task<Programa?> ObtenerAsync(int id)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.QueryFirstOrDefaultAsync<Programa>("SELECT * FROM programa WHERE id = @Id AND activo = 1", new { Id = id });
    }

    public async Task<bool> ExisteAsync(int id)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        var total = await conexion.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM programa WHERE id = @Id AND activo = 1", new { Id = id });
        return total > 0;
    }

    public async Task<int> CrearAsync(Programa programa)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        return await conexion.ExecuteScalarAsync<int>("INSERT INTO programa (nombre, codigo, activo) VALUES (@Nombre, @Codigo, 1); SELECT CAST(SCOPE_IDENTITY() as int);", programa);
    }

    public async Task<bool> ActualizarAsync(Programa programa)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        var res = await conexion.ExecuteAsync("UPDATE programa SET nombre = @Nombre, codigo = @Codigo WHERE id = @Id AND activo = 1", programa);
        return res > 0;
    }

    public async Task<bool> EliminarAsync(int id)
    {
        using var conexion = new SqlConnection(_cadenaConexion);
        var res = await conexion.ExecuteAsync("UPDATE programa SET activo = 0 WHERE id = @Id", new { Id = id });
        return res > 0;
    }
}
