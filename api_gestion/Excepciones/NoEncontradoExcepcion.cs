namespace ApiGestion.Excepciones;

public class NoEncontradoExcepcion : Exception
{
    public NoEncontradoExcepcion(string mensaje) : base(mensaje) { }
}