<<<<<<< HEAD
using ApiGestion.Modelos;
using Dapper;
using Microsoft.Data.SqlClient;
=======
using System.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using ApiGestion.Modelos;
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10

namespace ApiGestion.Repositorios;

public class RepositorioProgramaSqlServer : IRepositorioPrograma
{
<<<<<<< HEAD
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
=======
    private readonly string _conexion;

    public RepositorioProgramaSqlServer(IConfiguration config)
    {
        _conexion = config.GetConnectionString("SqlServer")!;
    }

    private IDbConnection CrearConexion() => new SqlConnection(_conexion);

    public async Task<List<Programa>> ObtenerTodosAsync(int limite = 100)
    {
        using var db = CrearConexion();
        var sql = @"SELECT TOP (@limite) 
                    id, nombre, tipo, nivel, fecha_creacion AS FechaCreacion, 
                    fecha_cierre AS FechaCierre, numero_cohortes AS NumeroCohortes, 
                    cant_graduados AS CantGraduados, fecha_actualizacion AS FechaActualizacion, 
                    ciudad, facultad, activo 
                    FROM programa 
                    WHERE activo = 1";
        var resultado = await db.QueryAsync<Programa>(sql, new { limite });
        return resultado.ToList();
    }

    public async Task<Programa?> ObtenerPorIdAsync(int id)
    {
        using var db = CrearConexion();
        var sql = @"SELECT id, nombre, tipo, nivel, fecha_creacion AS FechaCreacion, 
                    fecha_cierre AS FechaCierre, numero_cohortes AS NumeroCohortes, 
                    cant_graduados AS CantGraduados, fecha_actualizacion AS FechaActualizacion, 
                    ciudad, facultad, activo 
                    FROM programa 
                    WHERE id = @id AND activo = 1";
        return await db.QueryFirstOrDefaultAsync<Programa>(sql, new { id });
    }

    public async Task CrearAsync(Programa entidad)
    {
        using var db = CrearConexion();
        var sql = @"INSERT INTO programa (id, nombre, tipo, nivel, fecha_creacion, fecha_cierre, 
                    numero_cohortes, cant_graduados, fecha_actualizacion, ciudad, facultad, activo) 
                    VALUES (@Id, @Nombre, @Tipo, @Nivel, @FechaCreacion, @FechaCierre, 
                    @NumeroCohortes, @CantGraduados, @FechaActualizacion, @Ciudad, @Facultad, 1)";
        await db.ExecuteAsync(sql, entidad);
    }

    public async Task<bool> ActualizarAsync(Programa entidad)
    {
        using var db = CrearConexion();
        var sql = @"UPDATE programa SET 
                    nombre = @Nombre, tipo = @Tipo, nivel = @Nivel, 
                    fecha_creacion = @FechaCreacion, fecha_cierre = @FechaCierre, 
                    numero_cohortes = @NumeroCohortes, cant_graduados = @CantGraduados, 
                    fecha_actualizacion = @FechaActualizacion, ciudad = @Ciudad, facultad = @Facultad 
                    WHERE id = @Id AND activo = 1";
        var filas = await db.ExecuteAsync(sql, entidad);
        return filas > 0;
    }

    public async Task<bool> EliminarLogicoAsync(int id)
    {
        using var db = CrearConexion();
        var sql = "UPDATE programa SET activo = 0 WHERE id = @id AND activo = 1";
        var filas = await db.ExecuteAsync(sql, new { id });
        return filas > 0;
    }
}
>>>>>>> 05bd3051e7b6c901b480ffddfc5120e1ee1c0d10
