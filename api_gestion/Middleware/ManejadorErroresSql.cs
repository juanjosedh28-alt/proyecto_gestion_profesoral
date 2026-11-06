using Microsoft.Data.SqlClient;

namespace ApiGestion.Middleware;

/// <summary>
/// Traduce los errores que SUBEN DESDE LA BASE DE DATOS a respuestas HTTP con
/// mensaje en castellano (v2). Complementa a <see cref="ManejadorErrores"/> de la v1,
/// que no se modifica.
///
/// POR QUÉ EXISTE: en la v2 las reglas (¿el padre existe?, ¿la pareja ya está?,
/// ¿se puede retirar?) las revisa el procedimiento almacenado. Cuando una falla,
/// el procedimiento hace THROW con un número propio, y la API tiene que responder
/// 409/422/404 — nunca 500, porque 500 significa "me caí" y aquí nadie se cayó.
///
/// | Número           | Quién lo lanza                              | HTTP |
/// |------------------|---------------------------------------------|------|
/// | 50001            | procedimiento: el padre no existe/retirado  | 409  |
/// | 50002            | procedimiento: repetido o retiro bloqueado  | 409  |
/// | 50003            | procedimiento: dato inválido                | 422  |
/// | 50004            | procedimiento: no encontrado                | 404  |
/// | 547              | SQL Server: clave foránea (red de seguridad)| 409  |
/// | 2627 / 2601      | SQL Server: clave duplicada                 | 409  |
/// | 2628 / 8152      | SQL Server: texto más largo que la columna  | 422  |
///
/// Cualquier otro error de SQL (por ejemplo, la base apagada) NO se traduce:
/// se vuelve a lanzar y lo atiende el manejador de la v1 como 500.
///
/// ORDEN EN Program.cs: se registra DESPUÉS de ManejadorErrores. El que se
/// registra después queda más cerca del controlador, así que la excepción
/// pasa primero por aquí; si este no la reconoce, sigue hacia el de la v1.
/// </summary>
public class ManejadorErroresSql
{
    private readonly RequestDelegate _siguiente;

    public ManejadorErroresSql(RequestDelegate siguiente) => _siguiente = siguiente;

    public async Task InvokeAsync(HttpContext contexto)
    {
        try
        {
            await _siguiente(contexto);
        }
        catch (SqlException e)
        {
            var traduccion = Traducir(e);
            if (traduccion is null) throw;   // no es un error previsible: que lo maneje la v1

            contexto.Response.StatusCode = traduccion.Value.Codigo;
            await contexto.Response.WriteAsJsonAsync(new { mensaje = traduccion.Value.Mensaje, detalle = Array.Empty<string>() });
        }
    }

    /// <summary>Devuelve el código HTTP y el mensaje para el usuario, o null si el error no es previsible.</summary>
    private static (int Codigo, string Mensaje)? Traducir(SqlException e)
    {
        // El primer error de la lista es el que lanzó el procedimiento con THROW.
        var mensajeDelProcedimiento = e.Errors.Count > 0 ? e.Errors[0].Message : e.Message;

        return e.Number switch
        {
            50001 or 50002 => (409, mensajeDelProcedimiento),
            50003 => (422, mensajeDelProcedimiento),
            50004 => (404, mensajeDelProcedimiento),
            547 => (409, "No se pudo guardar: un dato relacionado no existe o el registro está en uso."),
            2627 or 2601 => (409, "Ya existe un registro con esa clave."),
            2628 or 8152 => (422, "Uno de los datos es más largo de lo permitido."),
            _ => null
        };
    }
}
