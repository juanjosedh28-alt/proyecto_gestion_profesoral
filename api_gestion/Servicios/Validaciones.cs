using System.Globalization;
using ApiGestion.Excepciones;

namespace ApiGestion.Servicios;

/// <summary>
/// Validaciones que comparten los servicios de la v2 (fechas y tamaño del envío múltiple).
///
/// POR QUÉ: las fechas viajan como texto "aaaa-mm-dd". La anotación
/// [RegularExpression] del DTO solo revisa la FORMA (que haya 4-2-2 dígitos);
/// "2024-13-45" tiene la forma correcta y no es una fecha. Aquí se revisa que
/// exista de verdad y que el fin no sea anterior al inicio. Si falla se lanza
/// ValidacionExcepcion, que el middleware de la v1 responde como 422.
/// </summary>
public static class Validaciones
{
    private const string Formato = "yyyy-MM-dd";

    /// <summary>Revisa una fecha. Si es opcional y viene vacía, devuelve null.</summary>
    public static DateTime? Revisar(string? valor, string campo, string etiqueta)
    {
        if (string.IsNullOrWhiteSpace(valor)) return null;
        if (!DateTime.TryParseExact(valor, Formato, CultureInfo.InvariantCulture, DateTimeStyles.None, out var fecha))
            throw new ValidacionExcepcion("Datos inválidos.", $"{campo}: {etiqueta} no es una fecha válida.");
        return fecha;
    }

    /// <summary>Revisa que la fecha final, si existe, no sea anterior a la inicial.</summary>
    public static void Orden(string inicio, string campoInicio, string? fin, string campoFin, string mensaje)
    {
        var desde = Revisar(inicio, campoInicio, "La fecha inicial");
        var hasta = Revisar(fin, campoFin, "La fecha final");
        if (desde is not null && hasta is not null && hasta < desde)
            throw new ValidacionExcepcion("Datos inválidos.", $"{campoFin}: {mensaje}");
    }

    /// <summary>Revisa que una fecha no esté en el futuro (por ejemplo, la de nacimiento).</summary>
    public static void NoFutura(string valor, string campo, string etiqueta)
    {
        var fecha = Revisar(valor, campo, etiqueta);
        if (fecha is not null && fecha > DateTime.Today)
            throw new ValidacionExcepcion("Datos inválidos.", $"{campo}: {etiqueta} no puede estar en el futuro.");
    }

    /// <summary>Revisa el tamaño de un envío de varios renglones (regla del 2_spec: de 1 a 100).</summary>
    public static void TamanoLote(int cantidad)
    {
        if (cantidad < 1 || cantidad > 100)
            throw new ValidacionExcepcion("El envío debe tener entre 1 y 100 renglones.");
    }
}
