namespace ApiGestion.Excepciones;

/// <summary>Se lanza cuando la clave o un campo único ya existe (incluso inactivo). La API responde 409.</summary>
public class ConflictoExcepcion : Exception
{
    public ConflictoExcepcion(string mensaje) : base(mensaje) { }
}
