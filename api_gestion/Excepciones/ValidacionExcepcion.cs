namespace ApiGestion.Excepciones;

/// <summary>Se lanza cuando una regla de negocio falla (fechas inválidas, id de ruta distinto al del cuerpo). La API responde 422.</summary>
public class ValidacionExcepcion : Exception
{
    public string[] Detalle { get; }

    public ValidacionExcepcion(string mensaje, params string[] detalle) : base(mensaje) => Detalle = detalle;
}
